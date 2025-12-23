using System.Linq;
using KinoAppCore.Abstractions;
using KinoAppDB;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Imdb;
using KinoAppShared.DTOs.Vorstellung;
using KinoAppShared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Periodically refreshes the film catalog from IMDb and seeds initial showings (Vorstellungen).
    /// </summary>
    /// <remarks>
    /// The service runs as a hosted background service. Film refresh and Vorstellung seeding are split into two
    /// transactional phases so that seeding operates on a committed film set.
    /// </remarks>
    public class FilmRefreshBackgroundService : BackgroundService
    {
        private readonly ILogger<FilmRefreshBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// Indicates whether Vorstellung seeding has already been executed in the current process.
        /// </summary>
        /// <remarks>
        /// This is an in-memory guard only. If you need cross-process or cross-instance safety, persist this state.
        /// </remarks>
        private bool _hasSeeded = false;

        /// <summary>
        /// Creates a new instance of the background service.
        /// </summary>
        /// <param name="logger">Logger used for operational and diagnostic messages.</param>
        /// <param name="scopeFactory">Factory used to create DI scopes for scoped services.</param>
        public FilmRefreshBackgroundService(
            ILogger<FilmRefreshBackgroundService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Executes the background loop until the host is shutting down.
        /// </summary>
        /// <param name="stoppingToken">Token that is signaled when the host is stopping.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Refreshing films from IMDb...");

                try
                {
                    // Phase 1: Refresh films and commit.
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var imdbService = scope.ServiceProvider.GetRequiredService<IImdbService>();
                        var dbScope = scope.ServiceProvider.GetRequiredService<IKinoAppDbContextScope>();

                        var refreshTimeout = TimeSpan.FromSeconds(30);

                        dbScope.Create();
                        await dbScope.BeginAsync(stoppingToken);

                        try
                        {
                            var importedMovies = await imdbService.ListMoviesAsync(
                                new ImdbListTitlesRequest(),
                                stoppingToken);

                            if (importedMovies == null || importedMovies.Count == 0)
                            {
                                _logger.LogWarning("IMDb API returned no movies — using fallback top 10 films.");
                                await AddFallbackFilms(scope.ServiceProvider, stoppingToken);
                            }

                            await dbScope.CommitAsync(stoppingToken);
                        }
                        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogInformation("IMDb refresh cancelled due to shutdown.");
                            await dbScope.RollbackAsync(CancellationToken.None);
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogWarning("IMDb refresh timed out after {Timeout}s.", refreshTimeout.TotalSeconds);
                            await dbScope.RollbackAsync(CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error while refreshing films from IMDb, rolling back.");
                            await dbScope.RollbackAsync(CancellationToken.None);
                        }
                    }

                    // Phase 2: Seed Vorstellungen once, based on committed films.
                    if (!_hasSeeded && !stoppingToken.IsCancellationRequested)
                    {
                        using var scope = _scopeFactory.CreateScope();

                        var dbScope2 = scope.ServiceProvider.GetRequiredService<IKinoAppDbContextScope>();

                        dbScope2.Create();
                        await dbScope2.BeginAsync(stoppingToken);

                        try
                        {
                            await SeedVorstellungenAsync(scope.ServiceProvider, stoppingToken);

                            await dbScope2.CommitAsync(stoppingToken);
                            _hasSeeded = true;
                        }
                        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogInformation("Vorstellung seeding cancelled due to shutdown.");
                            await dbScope2.RollbackAsync(CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error while seeding Vorstellungen, rolling back.");
                            await dbScope2.RollbackAsync(CancellationToken.None);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in FilmRefreshBackgroundService main loop.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Host is stopping.
                }
            }
        }

        /// <summary>
        /// Seeds showings (Vorstellungen) for the next few days, ensuring a minimum number of showings per hall.
        /// </summary>
        /// <param name="sp">Service provider used to resolve repositories and domain services.</param>
        /// <param name="ct">Cancellation token for the operation.</param>
        /// <remarks>
        /// The seeding logic operates on UTC timestamps and avoids scheduling the same film in overlapping time
        /// windows across halls by using an in-memory plan per day.
        /// </remarks>
        private async Task SeedVorstellungenAsync(IServiceProvider sp, CancellationToken ct)
        {
            var kinoRepo = sp.GetRequiredService<IKinosaalRepository>();
            var filmRepo = sp.GetRequiredService<IFilmRepository>();
            var vorstellungService = sp.GetRequiredService<IVorstellungService>();

            var halls = await kinoRepo.GetAllAsync(ct);
            if (halls.Count == 0)
                return;

            var films = (await filmRepo.GetAllAsync(ct))
                .Take(5)
                .ToList();

            if (films.Count == 0)
            {
                _logger.LogWarning("No films available — skipping Vorstellung seeding.");
                return;
            }

            var todayUtc = DateTime.UtcNow.Date;

            var days = new[]
            {
                todayUtc,
                todayUtc.AddDays(1),
                todayUtc.AddDays(2)
            };

            var nowUtc = DateTime.UtcNow;

            var dayPlans = new Dictionary<DateTime, List<PlannedShow>>();

            foreach (var day in days)
            {
                var shows = await vorstellungService.GetVorstellungVonTagAsync(day, ct);
                var list = new List<PlannedShow>();

                foreach (var v in shows)
                {
                    var dur = GetRuntimeMinutes(v.Film?.Dauer);
                    list.Add(new PlannedShow
                    {
                        FilmId = v.Film.Id,
                        StartUtc = v.Datum,
                        DurationMinutes = dur
                    });
                }

                dayPlans[day] = list;
            }

            foreach (var hall in halls)
            {
                foreach (var day in days)
                {
                    var existingForHall = await vorstellungService.GetVorstellungVonKinosaalUndTagAsync(day, hall.Id, ct);
                    var already = existingForHall.Count;
                    if (already >= 5)
                        continue;

                    var toCreate = 5 - already;
                    if (toCreate <= 0)
                        continue;

                    var plan = dayPlans[day];

                    DateTime startTimeUtc;
                    if (day == todayUtc)
                    {
                        startTimeUtc = RoundToNextQuarter(nowUtc);
                    }
                    else
                    {
                        startTimeUtc = new DateTime(day.Year, day.Month, day.Day, 14, 0, 0, DateTimeKind.Utc);
                    }

                    if (existingForHall.Any())
                    {
                        var last = existingForHall.OrderBy(v => v.Datum).Last();
                        var lastRuntime = GetRuntimeMinutes(last.Film?.Dauer);
                        startTimeUtc = RoundToNextQuarter(last.Datum.AddMinutes(lastRuntime));
                    }

                    for (int slot = 0; slot < toCreate; slot++)
                    {
                        if (startTimeUtc > day.AddDays(1).AddHours(6))
                        {
                            _logger.LogWarning(
                                "Stopping seeding for hall {HallId} on {Day} because startTimeUtc={Start}",
                                hall.Id, day, startTimeUtc);
                            break;
                        }

                        FilmEntity? selectedFilm = null;
                        int selectedRuntime = 0;

                        foreach (var candidate in films)
                        {
                            var runtime = GetRuntimeMinutes(candidate.Dauer);
                            if (!IsFilmRunningAtTime(candidate.Id, startTimeUtc, runtime, plan))
                            {
                                selectedFilm = candidate;
                                selectedRuntime = runtime;
                                break;
                            }
                        }

                        if (selectedFilm == null)
                        {
                            selectedFilm = films[0];
                            selectedRuntime = GetRuntimeMinutes(selectedFilm.Dauer);
                        }

                        var dto = new CreateVorstellungDTO
                        {
                            Datum = startTimeUtc,
                            FilmId = selectedFilm.Id,
                            KinosaalId = hall.Id,
                            Status = VorstellungStatus.GEPLANT
                        };

                        try
                        {
                            await vorstellungService.CreateVorstellungAsync(dto, ct);

                            plan.Add(new PlannedShow
                            {
                                FilmId = selectedFilm.Id,
                                StartUtc = startTimeUtc,
                                DurationMinutes = selectedRuntime
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Skipping Vorstellung due to conflict: {Message}", ex.Message);
                        }

                        startTimeUtc = RoundToNextQuarter(startTimeUtc.AddMinutes(selectedRuntime));
                    }
                }
            }
        }

        /// <summary>
        /// Seeds a small, predefined film catalog if the IMDb refresh produced no results and the database is empty.
        /// </summary>
        /// <param name="sp">Service provider used to resolve repositories.</param>
        /// <param name="ct">Cancellation token for the operation.</param>
        /// <remarks>
        /// This method only inserts films when the film table is empty to avoid polluting an already populated catalog.
        /// </remarks>
        private async Task AddFallbackFilms(IServiceProvider sp, CancellationToken ct)
        {
            var repo = sp.GetRequiredService<IFilmRepository>();

            var hasAny = await repo.Query().AnyAsync(ct);
            if (hasAny)
            {
                _logger.LogInformation("Film table already contains data — fallback seeding skipped.");
                return;
            }

            var fallback = new List<FilmEntity>
            {
                new FilmEntity
                {
                    Id = "godfather1972",
                    Titel = "The Godfather",
                    Beschreibung = "When aging patriarch Vito Corleone is nearly assassinated, reluctant son Michael is drawn into the violent world of the family business and slowly transforms into a ruthless mob boss.",
                    Dauer = 175,
                    Fsk = 16,
                    Genre = "Crime, Drama",
                    ImageURL = "https://m.media-amazon.com/images/M/MV5BM2MyNjYxNmYtNTAwMC00ZjQ5LWI5ZTItY2FjYzNlYWFhMWQxXkEyXkFqcGdeQXVyNjU0OTQ0OTY@._V1_.jpg"
                },
                new FilmEntity
                {
                    Id = "shawshank1994",
                    Titel = "The Shawshank Redemption",
                    Beschreibung = "Wrongly convicted banker Andy Dufresne forms an unlikely friendship with fellow inmate Red and quietly plans his escape while bringing a sense of hope to the brutal Shawshank prison.",
                    Dauer = 142,
                    Fsk = 16,
                    Genre = "Drama",
                    ImageURL = "https://m.media-amazon.com/images/I/51NiGlapXlL._AC_.jpg"
                },
                new FilmEntity
                {
                    Id = "darkknight2008",
                    Titel = "The Dark Knight",
                    Beschreibung = "Batman, Lieutenant Gordon and DA Harvey Dent wage war on Gotham’s organized crime until the Joker unleashes chaos, forcing Batman to confront the limits of justice and sacrifice.",
                    Dauer = 152,
                    Fsk = 16,
                    Genre = "Action, Crime, Drama",
                    ImageURL = "https://m.media-amazon.com/images/I/51k0qa6OzJL._AC_.jpg"
                },
                new FilmEntity
                {
                    Id = "godfather2",
                    Titel = "The Godfather: Part II",
                    Beschreibung = "The story intercuts Michael Corleone’s cold consolidation of power with flashbacks to young Vito Corleone’s rise in early 20th-century America, painting a tragic portrait of a family empire.",
                    Dauer = 202,
                    Fsk = 16,
                    Genre = "Crime, Drama",
                    ImageURL = "https://m.media-amazon.com/images/I/71xZ08nq1aL._AC_SY679_.jpg"
                },
                new FilmEntity
                {
                    Id = "12angry1957",
                    Titel = "12 Angry Men",
                    Beschreibung = "In a sweltering jury room, one lone juror questions the evidence in a murder trial, slowly forcing his peers to confront their biases and re-examine what they believe is an open-and-shut case.",
                    Dauer = 95,
                    Fsk = 12,
                    Genre = "Drama",
                    ImageURL = "https://m.media-amazon.com/images/I/81O1oy0S3-L._AC_SY679_.jpg"
                },
                new FilmEntity
                {
                    Id = "schindler1993",
                    Titel = "Schindler's List",
                    Beschreibung = "German businessman Oskar Schindler initially profits from World War II, but as he witnesses the horror of the Holocaust, he risks his fortune and life to save his Jewish workers.",
                    Dauer = 195,
                    Fsk = 16,
                    Genre = "Biography, Drama, History",
                    ImageURL = "https://m.media-amazon.com/images/I/51EbJjlLgJL._AC_.jpg"
                },
                new FilmEntity
                {
                    Id = "lotr3",
                    Titel = "The Lord of the Rings: The Return of the King",
                    Beschreibung = "While Frodo and Sam struggle to destroy the One Ring in Mordor, Aragorn must embrace his destiny and unite the free peoples of Middle-earth for the final battle against Sauron.",
                    Dauer = 201,
                    Fsk = 12,
                    Genre = "Adventure, Drama, Fantasy",
                    ImageURL = "https://m.media-amazon.com/images/I/51Qvs9i5a%2BL._AC_.jpg"
                },
                new FilmEntity
                {
                    Id = "pulpfiction1994",
                    Titel = "Pulp Fiction",
                    Beschreibung = "Interlocking tales of hitmen, a boxer, a mob boss and his wife collide in a darkly comic, non-linear crime story that reshapes the rules of storytelling in modern cinema.",
                    Dauer = 154,
                    Fsk = 18,
                    Genre = "Crime, Drama",
                    ImageURL = "https://m.media-amazon.com/images/I/71c05lTE03L._AC_SY679_.jpg"
                },
                new FilmEntity
                {
                    Id = "fightclub1999",
                    Titel = "Fight Club",
                    Beschreibung = "An insomniac office worker and a charismatic soap salesman form an underground fight club that spirals into an anarchistic movement challenging consumer culture and identity.",
                    Dauer = 139,
                    Fsk = 18,
                    Genre = "Drama",
                    ImageURL = "https://m.media-amazon.com/images/I/81D+2d6JtCL._AC_SY679_.jpg"
                },
                new FilmEntity
                {
                    Id = "forrest1994",
                    Titel = "Forrest Gump",
                    Beschreibung = "Simple but big-hearted Forrest Gump unwittingly drifts through key moments of American history while never losing sight of his love for Jenny and his own quiet sense of purpose.",
                    Dauer = 142,
                    Fsk = 12,
                    Genre = "Drama, Romance",
                    ImageURL = "https://m.media-amazon.com/images/I/61+ZRjjtELL._AC_SY679_.jpg"
                }
            };

            foreach (var film in fallback)
            {
                bool exists = await repo.AnyAsync(f => f.Id == film.Id, ct);
                if (!exists)
                    await repo.AddAsync(film, ct);
            }

            await repo.SaveAsync(ct);

            _logger.LogInformation("Fallback film data seeded successfully.");
        }

        /// <summary>
        /// Normalizes a runtime value to minutes.
        /// </summary>
        /// <param name="dauer">
        /// The runtime value as stored on the film. Values may be minutes or seconds depending on the source.
        /// </param>
        /// <returns>The runtime in minutes.</returns>
        /// <remarks>
        /// The domain intends <c>Dauer</c> to be minutes. Imported IMDb data may be provided in seconds.
        /// Values that look unrealistic for minutes are treated as seconds and converted.
        /// </remarks>
        private static int GetRuntimeMinutes(int? dauer)
        {
            if (!dauer.HasValue)
                return 120;

            var d = dauer.Value;

            if (d > 300)
                return d / 60;

            return d;
        }

        /// <summary>
        /// Rounds a timestamp up to the next quarter-hour boundary (:00, :15, :30, :45).
        /// </summary>
        /// <param name="dt">The timestamp to round.</param>
        /// <returns>The rounded timestamp, preserving the original <see cref="DateTime.Kind"/>.</returns>
        private static DateTime RoundToNextQuarter(DateTime dt)
        {
            var kind = dt.Kind;

            int minutes = dt.Minute;
            int nextQuarter = ((minutes / 15) + 1) * 15;

            if (nextQuarter >= 60)
            {
                dt = dt.AddHours(1);
                nextQuarter = 0;
            }

            return new DateTime(
                dt.Year, dt.Month, dt.Day,
                dt.Hour, nextQuarter, 0,
                kind);
        }

        /// <summary>
        /// Determines whether the specified film overlaps an existing planned showing within the given schedule.
        /// </summary>
        /// <param name="filmId">The film identifier to check.</param>
        /// <param name="startUtc">Proposed start time (UTC).</param>
        /// <param name="durationMinutes">Proposed duration in minutes.</param>
        /// <param name="plan">Existing planned showings for the day.</param>
        /// <returns><c>true</c> if an overlap exists; otherwise <c>false</c>.</returns>
        private static bool IsFilmRunningAtTime(string filmId, DateTime startUtc, int durationMinutes, List<PlannedShow> plan)
        {
            var endUtc = startUtc.AddMinutes(durationMinutes);

            foreach (var p in plan.Where(p => p.FilmId == filmId))
            {
                var pEnd = p.StartUtc.AddMinutes(p.DurationMinutes);
                if (startUtc < pEnd && endUtc > p.StartUtc)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// In-memory representation of a scheduled showing used to prevent overlaps during seeding.
        /// </summary>
        private sealed class PlannedShow
        {
            /// <summary>
            /// The film identifier.
            /// </summary>
            public string FilmId { get; set; } = string.Empty;

            /// <summary>
            /// The showing start time (UTC).
            /// </summary>
            public DateTime StartUtc { get; set; }

            /// <summary>
            /// The showing duration in minutes.
            /// </summary>
            public int DurationMinutes { get; set; }
        }
    }
}

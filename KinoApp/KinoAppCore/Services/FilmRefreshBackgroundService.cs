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
    public class FilmRefreshBackgroundService : BackgroundService
    {
        private readonly ILogger<FilmRefreshBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        // to make sure we only seed once per process
        private bool _hasSeeded = false;

        public FilmRefreshBackgroundService(
            ILogger<FilmRefreshBackgroundService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // simple loop – adjust delay if you want
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Refreshing films from IMDb...");

                try
                {
                    // ------------------------------
                    // PHASE 1: Refresh films & COMMIT
                    // ------------------------------
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

                            // If the API returned 0 films → load fallback instead
                            if (importedMovies == null || importedMovies.Count == 0)
                            {
                                _logger.LogWarning("IMDb API returned no movies — using fallback top 10 films.");
                                await AddFallbackFilms(scope.ServiceProvider, stoppingToken);
                            }

                            // ✅ commit film changes first
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
                            _logger.LogError(ex, "Error while refreshing films from IMDb (phase 1), rolling back.");
                            await dbScope.RollbackAsync(CancellationToken.None);
                        }
                    }

                    // ------------------------------
                    // PHASE 2: Seed Vorstellungen ONCE (uses committed films)
                    // ------------------------------
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
                            _logger.LogError(ex, "Error while seeding Vorstellungen (phase 2), rolling back.");
                            await dbScope2.RollbackAsync(CancellationToken.None);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in FilmRefreshBackgroundService main loop.");
                }

                // wait 1 day before next refresh
                try
                {
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // service is stopping
                }
            }
        }

        // --------------------------------------------------------------------
        // VORSTELLUNG SEEDING
        // --------------------------------------------------------------------
        private async Task SeedVorstellungenAsync(IServiceProvider sp, CancellationToken ct)
        {
            var kinoRepo = sp.GetRequiredService<IKinosaalRepository>();
            var filmRepo = sp.GetRequiredService<IFilmRepository>();
            var vorstellungService = sp.GetRequiredService<IVorstellungService>();

            // Load all halls
            var halls = await kinoRepo.GetAllAsync(ct);
            if (halls.Count == 0)
                return; // nothing to seed

            // Load films (take 5 max)
            var films = (await filmRepo.GetAllAsync(ct))
                .Take(5)
                .ToList();

            if (films.Count == 0)
            {
                _logger.LogWarning("No films available — skipping Vorstellung seeding.");
                return;
            }

            // ---- use UTC dates here ----
            var todayUtc = DateTime.UtcNow.Date;

            // Days to seed: today, tomorrow, day after tomorrow (all UTC)
            var days = new[]
            {
                todayUtc,
                todayUtc.AddDays(1),
                todayUtc.AddDays(2)
            };

            var nowUtc = DateTime.UtcNow;

            // In-memory schedule per day: all showings (existing + planned)
            var dayPlans = new Dictionary<DateTime, List<PlannedShow>>();

            // Preload existing Vorstellungen per day
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
                    // Check if this hall already has at least 5 shows that day
                    var existingForHall = await vorstellungService.GetVorstellungVonKinosaalUndTagAsync(day, hall.Id, ct);
                    var already = existingForHall.Count;
                    if (already >= 5)
                        continue; // already done

                    var toCreate = 5 - already;
                    if (toCreate <= 0)
                        continue;

                    var plan = dayPlans[day];

                    // First show time for this day
                    DateTime startTimeUtc;
                    if (day == todayUtc)
                    {
                        startTimeUtc = RoundToNextQuarter(nowUtc);
                    }
                    else
                    {
                        // default: 14:00 UTC for non-today
                        startTimeUtc = new DateTime(
                            day.Year, day.Month, day.Day,
                            14, 0, 0,
                            DateTimeKind.Utc);
                    }

                    // If this hall already has shows, continue after the last one
                    if (existingForHall.Any())
                    {
                        var last = existingForHall.OrderBy(v => v.Datum).Last();
                        var lastRuntime = GetRuntimeMinutes(last.Film?.Dauer);
                        startTimeUtc = RoundToNextQuarter(last.Datum.AddMinutes(lastRuntime));
                    }

                    // Create up to "toCreate" Vorstellungen for this hall/day
                    for (int slot = 0; slot < toCreate; slot++)
                    {
                        // hard safety: if we somehow ended up more than 24h after the day start, stop
                        if (startTimeUtc > day.AddDays(1).AddHours(6)) // e.g. after next day 06:00
                        {
                            _logger.LogWarning("Stopping seeding for hall {HallId} on {Day} because startTimeUtc={Start}",
                                hall.Id, day, startTimeUtc);
                            break;
                        }

                        FilmEntity? selectedFilm = null;
                        int selectedRuntime = 0;

                        // Pick first film that isn't already running at this time (any hall)
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

                        // Fallback: if somehow none fit, just take the first
                        if (selectedFilm == null)
                        {
                            selectedFilm = films[0];
                            selectedRuntime = GetRuntimeMinutes(selectedFilm.Dauer);
                        }

                        var dto = new CreateVorstellungDTO
                        {
                            Datum = startTimeUtc,  // already UTC
                            FilmId = selectedFilm.Id,
                            KinosaalId = hall.Id,
                            Status = VorstellungStatus.GEPLANT
                        };

                        try
                        {
                            await vorstellungService.CreateVorstellungAsync(dto, ct);

                            // Add to in-memory plan so later halls/slots see it
                            plan.Add(new PlannedShow
                            {
                                FilmId = selectedFilm.Id,
                                StartUtc = startTimeUtc,
                                DurationMinutes = selectedRuntime
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Skipping Vorstellung due to conflict: {ex.Message}");
                        }

                        // Move to next slot in this hall
                        startTimeUtc = RoundToNextQuarter(startTimeUtc.AddMinutes(selectedRuntime));
                    }
                }
            }
        }

        // --------------------------------------------------------------------
        // FALLBACK FILMS
        // --------------------------------------------------------------------
        private async Task AddFallbackFilms(IServiceProvider sp, CancellationToken ct)
        {
            var repo = sp.GetRequiredService<IFilmRepository>();

            // 1) Check if the entire film table is empty
            var hasAny = await repo.Query().AnyAsync(ct);
            if (hasAny)
            {
                _logger.LogInformation("Film table already contains data — fallback seeding skipped.");
                return;
            }

            // 2) Fallback dataset: Top 10 movies (basic info)
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

            // 3) Insert fallback films (DB is known to be empty)
            foreach (var film in fallback)
            {
                bool exists = await repo.AnyAsync(f => f.Id == film.Id, ct);
                if (!exists)
                    await repo.AddAsync(film, ct);
            }

            await repo.SaveAsync(ct);

            _logger.LogInformation("Fallback film data seeded successfully.");
        }

        // --------------------------------------------------------------------
        // HELPER METHODS & TYPES
        // --------------------------------------------------------------------

        /// <summary>
        /// Dauer is intended to be in minutes, but imported IMDb data may be in seconds.
        /// If the value is very large, treat it as seconds and convert to minutes.
        /// </summary>
        private static int GetRuntimeMinutes(int? dauer)
        {
            if (!dauer.HasValue)
                return 120; // default 2h

            var d = dauer.Value;

            // If it looks like seconds (e.g. 5400), convert to minutes.
            if (d > 300) // > 5h as minutes is unrealistic
                return d / 60;

            return d; // already minutes
        }

        /// <summary>
        /// Round up to the next :00 / :15 / :30 / :45, keeping DateTimeKind.Utc.
        /// </summary>
        private static DateTime RoundToNextQuarter(DateTime dt)
        {
            var kind = dt.Kind; // keep Local/Utc/Unspecified

            int minutes = dt.Minute;
            int nextQuarter = ((minutes / 15) + 1) * 15;

            // if we roll over to the next hour (or beyond), move dt forward by that hour
            if (nextQuarter >= 60)
            {
                // move dt to the next whole hour, this will correctly roll over day/month/year
                dt = dt.AddHours(1);
                nextQuarter = 0;
            }

            // rebuild with correct date/time and original Kind
            return new DateTime(
                dt.Year, dt.Month, dt.Day,
                dt.Hour, nextQuarter, 0,
                kind);
        }


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

        private sealed class PlannedShow
        {
            public string FilmId { get; set; } = string.Empty;
            public DateTime StartUtc { get; set; }
            public int DurationMinutes { get; set; }
        }
    }
}

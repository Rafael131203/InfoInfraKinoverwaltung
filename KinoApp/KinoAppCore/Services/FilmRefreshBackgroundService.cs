using KinoAppCore.Abstractions;
using KinoAppDB;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Imdb;
using KinoAppShared.DTOs.Kinosaal;
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
                    using var scope = _scopeFactory.CreateScope();

                    // resolve scoped/transient services INSIDE the scope
                    var imdbService = scope.ServiceProvider.GetRequiredService<IImdbService>();
                    var dbScope = scope.ServiceProvider.GetRequiredService<IKinoAppDbContextScope>();

                    // seeding dependencies
                    var preisRepo = scope.ServiceProvider.GetRequiredService<IRepository<PreisZuKategorieEntity>>();
                    var preisSvc = scope.ServiceProvider.GetRequiredService<IPreisZuKategorieService>();

                    var refreshTimeout = TimeSpan.FromSeconds(30);

                    // mimic your BaseController transaction pattern
                    dbScope.Create();
                    await dbScope.BeginAsync(stoppingToken);

                    try
                    {
                        // 1) Seed PreisZuKategorie ONCE (inside same transaction, sequentially)
                        if (!_hasSeeded)
                        {
                            await SeedPreisZuKategorieAsync(preisRepo, preisSvc, stoppingToken);
                            _hasSeeded = true;
                        }

                        // 2) Do your IMDb refresh
                        await imdbService.ListMoviesAsync(new ImdbListTitlesRequest(), stoppingToken);

                        await dbScope.CommitAsync(stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("IMDb refresh cancelled due to shutdown.");
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("IMDb refresh timed out after {Timeout}s.", refreshTimeout.TotalSeconds);
                        await dbScope.RollbackAsync(CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in FilmRefreshBackgroundService, rolling back.");
                        await dbScope.RollbackAsync(CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while refreshing films from IMDb.");
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

        /// <summary>
        /// Seeds PreisZuKategorie if table is empty.
        /// Uses the same DbContext behind IKinoAppDbContextScope via the repository and service.
        /// </summary>
        private static async Task SeedPreisZuKategorieAsync(IRepository<PreisZuKategorieEntity> preisRepo, IPreisZuKategorieService preisService, CancellationToken ct)
        {
            // already has data? -> do nothing
            var any = await preisRepo.Query().AnyAsync(ct);
            if (any)
                return;

            // IMPORTANT: sequential awaits, no Task.WhenAll -> avoids "second operation" errors
            await preisService.SetPreisAsync(
                new SetPreisDTO
                {
                    Kategorie = SitzreihenKategorie.Parkett,
                    Preis = 15.00m
                },
                ct);

            await preisService.SetPreisAsync(
                new SetPreisDTO
                {
                    Kategorie = SitzreihenKategorie.LOGE,
                    Preis = 25.50m
                },
                ct);

            await preisService.SetPreisAsync(
                new SetPreisDTO
                {
                    Kategorie = SitzreihenKategorie.LOGEPLUS,
                    Preis = 40.00m
                },
                ct);
        }
    }
}

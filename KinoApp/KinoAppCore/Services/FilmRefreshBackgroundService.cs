using KinoAppCore.Services;
using KinoAppDB;
using KinoAppShared.DTOs.Imdb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KinoAppCore.Services
{
    public class FilmRefreshBackgroundService : BackgroundService
    {
        private readonly ILogger<FilmRefreshBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

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

                    var refreshTimeout = TimeSpan.FromSeconds(30);

                    // mimic your BaseController transaction pattern
                    dbScope.Create();
                    await dbScope.BeginAsync(stoppingToken);

                    try
                    {
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
                    }
                    catch (Exception ex)
                    {
                        await dbScope.RollbackAsync(CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while refreshing films from IMDb.");
                }

                // wait 1 hour before next refresh
                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // service is stopping
                }
            }
        }
    }
}

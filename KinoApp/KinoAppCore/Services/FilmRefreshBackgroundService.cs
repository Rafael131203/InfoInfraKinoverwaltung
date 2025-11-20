using KinoAppCore.Services;
using KinoAppDB;
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

                    // mimic your BaseController transaction pattern
                    dbScope.Create();
                    await dbScope.BeginAsync(stoppingToken);

                    try
                    {
                        await imdbService.RefreshAllFilmsAsync(stoppingToken);

                        await dbScope.CommitAsync(stoppingToken);
                    }
                    catch
                    {
                        await dbScope.RollbackAsync(stoppingToken);
                        throw;
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

using KinoAppCore.Abstractions;
using KinoAppCore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KinoAppCore
{
    /// <summary>
    /// Dependency injection extensions for registering KinoApp core services.
    /// </summary>
    /// <remarks>
    /// This registration is intentionally limited to core application services. Infrastructure concerns such as
    /// EF Core DbContexts, repositories, token implementations, MongoDB, and message bus bindings are expected
    /// to be configured by the hosting layer.
    /// </remarks>
    public static class KinoAppCoreServiceExtension
    {
        /// <summary>
        /// Registers the core application services into the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection to register services with.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public static IServiceCollection AddKinoAppCore(this IServiceCollection services)
        {
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IKinosaalService, KinosaalService>();
            services.AddScoped<IPreisZuKategorieService, PreisZuKategorieService>();
            services.AddScoped<IVorstellungService, VorstellungService>();
            services.AddScoped<IImdbService, ImdbService>();

            return services;
        }
    }
}

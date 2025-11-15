using KinoAppCore.Abstractions;
//using KinoAppCore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KinoAppCore;

/// <summary>
/// Registers only CORE services (no EF/JWT/Mongo/MassTransit here).
/// </summary>
public static class KinoAppCoreServiceExtension
{
    public static IServiceCollection AddKinoAppCore(this IServiceCollection services)
    {
        // Use-cases / business services
        //services.AddScoped<IAuthService, AuthService>();
        //services.AddScoped<IBookingService, BookingService>();

        // Nothing infra-related here. Repositories, token service, message bus
        // are bound in the Service project where their implementations live.
        return services;
    }
}

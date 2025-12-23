using System.Net.Http;
using KinoAppCore.Services;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace KinoAppWeb
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            var apiBase = new Uri("http://localhost:5170/"); // adjust if needed

            // Session + auth
            builder.Services.AddScoped<UserSession>();
            builder.Services.AddScoped<JwtAuthHandler>();

            // HttpClient used for anonymous endpoints (login/register/refresh).
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiBase });

            builder.Services.AddScoped<IClientLoginService, ClientLoginService>();
            builder.Services.AddScoped<IMovieShowtimeService, MovieShowtimeService>();
            builder.Services.AddScoped<ITicketApiService, TicketApiService>();
            builder.Services.AddScoped<ImdbApiClient>();

            // Authenticated API clients (attach bearer token automatically).
            builder.Services.AddHttpClient<Services.IVorstellungService, Services.VorstellungService>(client =>
            {
                client.BaseAddress = apiBase;
            }).AddHttpMessageHandler<JwtAuthHandler>();

            builder.Services.AddHttpClient<Services.IKinosaalService, Services.KinosaalService>(client =>
            {
                client.BaseAddress = apiBase;
            }).AddHttpMessageHandler<JwtAuthHandler>();

            await builder.Build().RunAsync();
        }
    }
}

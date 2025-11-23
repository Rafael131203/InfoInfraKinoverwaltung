using KinoAppCore.Services;
using KinoAppWeb;
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

            // HttpClient for API (KinoAppService)
            // Make sure this matches the URL/port where your ASP.NET API runs
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5170/") // adjust if needed
            });

            // Client auth + session
            builder.Services.AddScoped<IMovieShowtimeService, MovieShowtimeService>();
            builder.Services.AddScoped<IClientLoginService, ClientLoginService>();
            builder.Services.AddScoped<Services.IKinosaalService, Services.KinosaalService>();
            builder.Services.AddScoped<Services.IVorstellungService, Services.VorstellungService>();
            builder.Services.AddScoped<UserSession>();

            // IMDb API client (talks to your API's /api/imdb endpoints)
            builder.Services.AddScoped<ImdbApiClient>();

            builder.Services.AddScoped<JwtAuthHandler>();


            builder.Services.AddHttpClient<Services.IVorstellungService, Services.VorstellungService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5170/"); // Deine API-URL
            })
            .AddHttpMessageHandler<JwtAuthHandler>();

            builder.Services.AddHttpClient<Services.IKinosaalService, Services.KinosaalService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5170/");
            })
            .AddHttpMessageHandler<JwtAuthHandler>();




            await builder.Build().RunAsync();
        }
    }
}

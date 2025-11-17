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

            // HttpClient for API
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5170/") // adjust if docker differs
            });

            // Client auth + session
            builder.Services.AddScoped<IClientLoginService, ClientLoginService>();
            builder.Services.AddScoped<UserSession>();

            await builder.Build().RunAsync();
        }
    }
}

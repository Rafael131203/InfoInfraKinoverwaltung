using Asp.Versioning;                          // remove if not using
using KinoAppCore;                             // Core DI extension
using KinoAppCore.Abstractions;
using KinoAppCore.Config;
using KinoAppCore.Services;
using Microsoft.Extensions.Options;
using KinoAppDB;                               // DbContext
using KinoAppDB.Repository;                    // Repositories
using KinoAppService.Messaging;                // IMessageBus adapter
using KinoAppService.Security;  
// JwtTokenService
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;

namespace KinoAppService
{
    internal static class Startup
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            // optional: load docker overrides
            var config = configuration;

            // CORS for Blazor WASM
            services.AddCors(o => o.AddPolicy("ui", p =>p.WithOrigins(
                        "https://localhost:7268",  // Blazor dev server (https profile)
                        "http://localhost:5143"    // Blazor dev server (http profile)
                    ).AllowAnyHeader().AllowAnyMethod()
            ));


            // ---------- AutoMapper ----------
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // ---------- PostgreSQL / EF Core ----------
            services.AddDbContextFactory<KinoAppDbContext>(o =>
                o.UseNpgsql(config.GetConnectionString("Postgres")));

            // ---------- MongoDB (local / Docker) ----------
            // Priority: env var "Mongo" → connection string "Mongo" → default localhost
            var mongoConnectionString =
                config["Mongo"] ??
                config.GetConnectionString("Mongo") ??
                "mongodb://localhost:27017";

            var mongoDatabaseName =
                config["MongoDatabase"] ?? "kino";

            services.AddSingleton<IMongoClient>(_ =>
                new MongoClient(mongoConnectionString));

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoDatabaseName);
            });

            // Our Mongo-based services
            services.AddScoped<TicketService>();
            services.AddScoped<StatsService>();

            // ---------- JWT (optional – leave here if you need it) ----------
            var keyString = config["Jwt:SigningKey"] ?? "dev-change-me";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = config["Jwt:Audience"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateLifetime = true
                    };
                });

            services.AddAuthorization();

            // ---------- Swagger ----------
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "KinoApp API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    { new OpenApiSecurityScheme {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }, new List<string>() }
                });
            });

            // ---------- Controllers & API Versioning ----------
            services.AddControllers();
            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
            }).AddMvc();

            // ---------- Core + DB Repos ----------
            services.AddKinoAppCore();

            // IMDb API config
            services.Configure<ApiOptions>(config.GetSection(ApiOptions.SectionName));

            // Typed HttpClient for IImdbService
            services.AddHttpClient<IImdbService, ImdbService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<ApiOptions>>().Value;

                if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/'));
                }
            });

            // Bind Core ports to infra implementations  // from KinoAppDB
            services.AddScoped<KinoAppDbContextScope>(); // concrete
            services.AddScoped((Func<IServiceProvider, IKinoAppDbContextScope>)(sp => sp.GetRequiredService<KinoAppDbContextScope>()));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IKundeRepository, KundeRepository>();  // from KinoAppDB
            services.AddScoped<IKinosaalRepository, KinosaalRepository>();  // from KinoAppDB
            services.AddScoped<ISitzreiheRepository, SitzreiheRepository>();  // from KinoAppDB
            services.AddScoped<ISitzplatzRepository, SitzplatzRepository>();  // from KinoAppDB
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
            //services.AddScoped<IMessageBus, MassTransitKafkaMessageBus>(); // from KinoAppService
            //services.AddScoped<ITokenService>(_ => new JwtTokenService(config["Jwt:Issuer"]!, config["Jwt:Audience"]!, key, TimeSpan.FromHours(8)));
        }

        public static void Configure(WebApplication app)
        {
            app.UseCors("ui");

            app.UseSwagger();
            app.UseSwaggerUI();

            // Uncomment once you actually secure endpoints
            //app.UseAuthentication();
            //app.UseAuthorization();

            app.MapControllers();

            // Optional: simple health check for Docker
            app.MapGet("/health", () => Results.Ok("OK"));

            // Optional: automatic migrations if you want
            //using var scope = app.Services.CreateScope();
            //var db = scope.ServiceProvider.GetRequiredService<KinoAppDbContext>();
            //db.Database.Migrate();
        }
    }
}

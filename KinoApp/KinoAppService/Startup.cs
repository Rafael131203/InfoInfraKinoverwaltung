using Asp.Versioning;                          // remove if not using
using Confluent.Kafka;
using KinoAppCore;                             // Core DI extension
using KinoAppCore.Abstractions;
using KinoAppCore.Config;
using KinoAppCore.Services;
using KinoAppDB;                               // DbContext
using KinoAppDB.Repository;                    // Repositories
using KinoAppService.Messaging;                // IMessageBus adapter
using KinoAppService.Security;
using KinoAppShared.Messaging;
// JwtTokenService
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Security.Claims;
using System.Text;

namespace KinoAppService
{
    internal static class Startup
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // optional: load docker overrides
            var config = configuration;

            // NEW: read current environment once
            var environmentName =
                config["ASPNETCORE_ENVIRONMENT"] ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                "Production";

            Console.WriteLine($"[Startup] ASPNETCORE_ENVIRONMENT = '{environmentName}'"); // NEW (for debugging)

            // CORS for Blazor WASM
            services.AddCors(o => o.AddPolicy("ui", p => p.WithOrigins(
                        "https://localhost:7268",  // Blazor dev server (https profile)
                        "http://localhost:5143"    // Blazor dev server (http profile)
                    ).AllowAnyHeader().AllowAnyMethod()
            ));


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

            // ---------- MassTransit + Redpanda (Kafka) ----------
            services.AddMassTransit(x =>
            {
                // Hauptbus: InMemory (wir nutzen den Kafka-Rider)
                x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));

                x.AddRider(r =>
                {
                    // Consumers registrieren
                    r.AddConsumer<TicketSoldProjectionConsumer>();
                    r.AddConsumer<KundeRegisteredConsumer>();
                    

                    // Producer registrieren (für IMessageBus)
                    r.AddProducer<TicketSold>("ticket-sold");
                    r.AddProducer<KundeRegistered>("kunde-registered");
                    r.AddProducer<ShowCreated>("show-created");
                    r.AddProducer<TicketCancelled>("ticket-cancelled");

                    r.UsingKafka((ctx, k) =>
                    {
                        // NEW: decide brokers based on environment
                        string brokers;
                        if (string.Equals(environmentName, "Docker", StringComparison.OrdinalIgnoreCase))
                        {
                            // inside Docker network -> redpanda is resolvable
                            brokers = config["Kafka:BootstrapServers"] ?? "redpanda:9092";
                        }
                        else
                        {
                            // local dev (Development / anything else) -> always localhost
                            brokers = "localhost:19092";
                            //brokers = "localhost:9092";
                        }

                        // original:
                        var groupId = config["Kafka:ConsumerGroup"] ?? "kinoapp-service";

                        // testing: unique group id each run, to always get all created messages
                        // var groupId = "kinoapp-debug-" + Guid.NewGuid().ToString();

                        Console.WriteLine($"[Kafka] Env='{environmentName}', BootstrapServers='{brokers}', GroupId='{groupId}'");

                        k.Host(brokers);

                        // TicketSold -> TicketSoldProjectionConsumer
                        k.TopicEndpoint<TicketSold>("ticket-sold", groupId, e =>
                        {
                            e.AutoOffsetReset = AutoOffsetReset.Earliest;
                            e.ConfigureConsumer<TicketSoldProjectionConsumer>(ctx);
                            e.CreateIfMissing();
                        });

                        // KundeRegistered -> KundeRegisteredConsumer
                        k.TopicEndpoint<KundeRegistered>("kunde-registered", groupId, e =>
                        {
                            e.AutoOffsetReset = AutoOffsetReset.Earliest;
                            e.ConfigureConsumer<KundeRegisteredConsumer>(ctx);
                            e.CreateIfMissing();
                        });

                        // NEU: Storno-Event registrieren
                        k.TopicEndpoint<TicketCancelled>("ticket-cancelled", groupId, e =>
                        {
                            e.AutoOffsetReset = AutoOffsetReset.Earliest;
                            e.ConfigureConsumer<TicketSoldProjectionConsumer>(ctx); // <--- Derselbe Consumer!
                            e.CreateIfMissing();
                        });
                    });
                });
            });

            // IMessageBus-Wrapper
            services.AddScoped<IMessageBus, MassTransitKafkaMessageBus>();
            // ----------------------------------------------------

            // JWT auth (remove if not needed yet)
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
                        ValidateLifetime = true,
                        RoleClaimType = ClaimTypes.Role
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
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            // Bind Core ports to infra implementations  // from KinoAppDB
            services.AddScoped<KinoAppDbContextScope>(); // concrete
            services.AddScoped((Func<IServiceProvider, IKinoAppDbContextScope>)(sp => sp.GetRequiredService<KinoAppDbContextScope>()));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();  // from KinoAppDB
            services.AddScoped<IKinosaalRepository, KinosaalRepository>();  // from KinoAppDB
            services.AddScoped<ISitzreiheRepository, SitzreiheRepository>();  // from KinoAppDB
            services.AddScoped<ISitzplatzRepository, SitzplatzRepository>();  // from KinoAppDB
            services.AddScoped<IVorstellungRepository, VorstellungRepository>();  // from KinoAppDB
            services.AddScoped<IFilmRepository, FilmRepository>();  // from KinoAppDB
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
            services.AddHostedService<FilmRefreshBackgroundService>();
        }

        public static void Configure(WebApplication app)
        {
            app.UseCors("ui");

            app.UseSwagger();
            app.UseSwaggerUI();

            // Uncomment once you actually secure endpoints
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }
    }
}

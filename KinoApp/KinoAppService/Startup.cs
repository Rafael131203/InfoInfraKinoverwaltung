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
    /// <summary>
    /// Central service and middleware wiring for the KinoApp service host.
    /// </summary>
    /// <remarks>
    /// The project uses a minimal hosting entry point (<c>Program.cs</c>) that delegates to this class.
    /// This keeps infrastructure concerns (EF, MongoDB, MassTransit/Kafka, JWT, Swagger, CORS) in one place.
    /// </remarks>
    internal static class Startup
    {
        /// <summary>
        /// Registers application services and infrastructure dependencies.
        /// </summary>
        /// <param name="services">Service collection for dependency injection.</param>
        /// <param name="configuration">Application configuration (appsettings, env vars, user secrets, etc.).</param>
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration;

            var environmentName =
                config["ASPNETCORE_ENVIRONMENT"] ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                "Production";

            Console.WriteLine($"[Startup] ASPNETCORE_ENVIRONMENT = '{environmentName}'");

            // -------------------- CORS (Blazor WASM UI) --------------------
            services.AddCors(o => o.AddPolicy("ui", p => p.WithOrigins(
                        "https://localhost:7268",
                        "http://localhost:5143"
                    ).AllowAnyHeader().AllowAnyMethod()
            ));

            // -------------------- AutoMapper --------------------
            // Scans loaded assemblies for Profile classes (Core mappings live in KinoAppCore.Mappings).
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // -------------------- PostgreSQL / EF Core --------------------
            services.AddDbContextFactory<KinoAppDbContext>(o =>
                o.UseNpgsql(config.GetConnectionString("Postgres")));

            // -------------------- MongoDB --------------------
            // Priority: env var "Mongo" -> connection string "Mongo" -> default localhost.
            var mongoConnectionString =
                config["Mongo"] ??
                config.GetConnectionString("Mongo") ??
                "mongodb://localhost:27017";

            var mongoDatabaseName =
                config["MongoDatabase"] ?? "kino";

            services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoDatabaseName);
            });

            // -------------------- MassTransit + Kafka (Redpanda) --------------------
            services.AddMassTransit(x =>
            {
                // Main bus is in-memory; Kafka is configured through the rider.
                x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));

                x.AddRider(r =>
                {
                    // Consumers
                    r.AddConsumer<TicketSoldProjectionConsumer>();
                    r.AddConsumer<KundeRegisteredConsumer>();

                    // Producers (IMessageBus publishes via these topic producers)
                    r.AddProducer<TicketSold>("ticket-sold");
                    r.AddProducer<KundeRegistered>("kunde-registered");
                    r.AddProducer<ShowCreated>("show-created");
                    r.AddProducer<TicketCancelled>("ticket-cancelled");

                    r.UsingKafka((ctx, k) =>
                    {
                        string brokers;
                        if (string.Equals(environmentName, "Docker", StringComparison.OrdinalIgnoreCase))
                        {
                            brokers = config["Kafka:BootstrapServers"] ?? "redpanda:9092";
                        }
                        else
                        {
                            brokers = "localhost:19092";
                        }

                        var groupId = config["Kafka:ConsumerGroup"] ?? "kinoapp-service";

                        Console.WriteLine($"[Kafka] Env='{environmentName}', BootstrapServers='{brokers}', GroupId='{groupId}'");

                        k.Host(brokers);

                        k.TopicEndpoint<TicketSold>("ticket-sold", groupId, e =>
                        {
                            e.AutoOffsetReset = AutoOffsetReset.Earliest;
                            e.ConfigureConsumer<TicketSoldProjectionConsumer>(ctx);
                            e.CreateIfMissing();
                        });

                        k.TopicEndpoint<KundeRegistered>("kunde-registered", groupId, e =>
                        {
                            e.AutoOffsetReset = AutoOffsetReset.Earliest;
                            e.ConfigureConsumer<KundeRegisteredConsumer>(ctx);
                            e.CreateIfMissing();
                        });

                        k.TopicEndpoint<TicketCancelled>("ticket-cancelled", groupId, e =>
                        {
                            e.AutoOffsetReset = AutoOffsetReset.Earliest;
                            e.ConfigureConsumer<TicketSoldProjectionConsumer>(ctx);
                            e.CreateIfMissing();
                        });
                    });
                });
            });

            // Message bus adapter used by Core services.
            services.AddScoped<IMessageBus, MassTransitKafkaMessageBus>();

            // Mongo-backed services
            services.AddScoped<TicketService>();
            services.AddScoped<StatsService>();

            // -------------------- JWT Authentication / Authorization --------------------
            // NOTE: JwtTokenService enforces a stronger key requirement; this fallback is kept for local dev.
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

            // -------------------- Swagger --------------------
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
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string>()
                    }
                });
            });

            // -------------------- Controllers & API Versioning --------------------
            services.AddControllers();

            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
            }).AddMvc();

            // -------------------- Core wiring --------------------
            services.AddKinoAppCore();

            services.Configure<ApiOptions>(config.GetSection(ApiOptions.SectionName));

            services.AddHttpClient<IImdbService, ImdbService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<ApiOptions>>().Value;

                if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/'));

                client.Timeout = TimeSpan.FromSeconds(10);
            });

            // -------------------- Infrastructure bindings (DB + Security) --------------------
            services.AddScoped<KinoAppDbContextScope>();
            services.AddScoped<IKinoAppDbContextScope>(sp => sp.GetRequiredService<KinoAppDbContextScope>());

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IKinosaalRepository, KinosaalRepository>();
            services.AddScoped<ISitzreiheRepository, SitzreiheRepository>();
            services.AddScoped<ISitzplatzRepository, SitzplatzRepository>();
            services.AddScoped<IVorstellungRepository, VorstellungRepository>();
            services.AddScoped<IFilmRepository, FilmRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();

            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

            services.AddHostedService<FilmRefreshBackgroundService>();
        }

        /// <summary>
        /// Configures the HTTP middleware pipeline for the application.
        /// </summary>
        /// <param name="app">Web application instance.</param>
        public static void Configure(WebApplication app)
        {
            app.UseCors("ui");

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }
    }
}

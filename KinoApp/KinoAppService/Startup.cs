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
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // optional: load docker overrides
            var config = configuration;

            // CORS for Blazor WASM
            services.AddCors(o => o.AddPolicy("ui", p =>p.WithOrigins(
                        "https://localhost:7268",  // Blazor dev server (https profile)
                        "http://localhost:5143"    // Blazor dev server (http profile)
                    ).AllowAnyHeader().AllowAnyMethod()
            ));


            //AutoMapper for DTO <-> Entity mapping
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // EF Core (Postgres)
            services.AddDbContextFactory<KinoAppDbContext>(o =>
                o.UseNpgsql(config.GetConnectionString("Postgres")));

            // Mongo (optional)
            services.AddSingleton<IMongoClient>(_ =>
                new MongoClient(config["Mongo"]));

            // MassTransit (Kafka Rider) — if you’re using Redpanda
            //services.AddMassTransit(x =>
            //{
            //    // Main bus not used → in-memory
            //    x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));

            //    x.AddRider(r =>
            //    {
            //        r.AddConsumer<TicketSoldProjectionConsumer>();               // your consumer
            //        r.AddProducer<KinoAppShared.Messaging.TicketSold>("ticket-sold"); // your producer topic

            //        r.UsingKafka((ctx, k) =>
            //        {
            //            var brokers = config["Kafka:Brokers"] ?? "redpanda:9092";
            //            var groupId = config["Kafka:ConsumerGroup"] ?? "kinoapp-service";
            //            k.Host(brokers);

            //            k.TopicEndpoint<KinoAppShared.Messaging.TicketSold>("ticket-sold", groupId, e =>
            //            {
            //                e.ConfigureConsumer<TicketSoldProjectionConsumer>(ctx);
            //            });
            //        });
            //    });
            //});

            // JWT auth (remove if not needed yet)
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

            // Swagger
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
                    { new OpenApiSecurityScheme { Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme, Id = "Bearer"}}, new List<string>() }
                });
            });

            // Controllers & (optional) API versioning
            services.AddControllers();
            // comment out if no versioning needed
            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
            }).AddMvc();

            // CORE services (pure logic)
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

            //app.UseAuthentication();
            //app.UseAuthorization();

            app.MapControllers();

            // dev/docker: apply EF migrations automatically
            //using var scope = app.Services.CreateScope();
            //var db = scope.ServiceProvider.GetRequiredService<KinoAppDbContext>();
            //db.Database.Migrate();
        }
    }
}

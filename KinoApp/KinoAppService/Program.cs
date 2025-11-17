using KinoAppCore.Mappings;
using KinoAppDB;
using KinoAppService; // <-- make sure this matches Startup.cs namespace
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text;

// Needed once if you deal with non-UTF8 encodings later
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);


// Binde die Einstellungen aus appsettings.json
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

// Registriere den MongoClient als Singleton
builder.Services.AddSingleton<MongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// Registriere die IMongoDatabase als Singleton
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var client = sp.GetRequiredService<MongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

// Hand off to Startup (keeps Program.cs tidy)
Startup.ConfigureServices(builder.Services, builder.Configuration);

builder.Services.AddAutoMapper(typeof(KundeMappingProfile));
builder.Services.AddAutoMapper(typeof(WarenkorbProfile));

var app = builder.Build();
Startup.Configure(app);

app.Run();

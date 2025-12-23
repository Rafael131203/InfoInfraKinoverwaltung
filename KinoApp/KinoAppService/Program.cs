using KinoAppCore.Mappings;
using KinoAppDB;
using KinoAppService;
using System.Text;

// Needed once if you deal with non-UTF8 encodings later
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// Hand off to Startup
Startup.ConfigureServices(builder.Services, builder.Configuration);

// Mapping profiles
builder.Services.AddAutoMapper(typeof(UserMappingProfile));
builder.Services.AddAutoMapper(typeof(KinosaalMappingProfile));
builder.Services.AddAutoMapper(typeof(SitzreiheMappingProfile));
builder.Services.AddAutoMapper(typeof(SitzplatzMappingProfile));
builder.Services.AddAutoMapper(typeof(PreisMappingProfile));
builder.Services.AddAutoMapper(typeof(VorstellungMappingProfile));

var app = builder.Build();

Startup.Configure(app);

app.Run();

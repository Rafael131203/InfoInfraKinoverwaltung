using KinoAppCore.Mappings;
using KinoAppService;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// Hand off to Startup (includes AutoMapper registration)
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

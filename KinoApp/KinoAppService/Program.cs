using KinoAppCore.Mappings;
using KinoAppService; // <-- make sure this matches Startup.cs namespace
using System.Text;

// Needed once if you deal with non-UTF8 encodings later
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// Hand off to Startup (keeps Program.cs tidy)
Startup.ConfigureServices(builder.Services, builder.Configuration);

builder.Services.AddAutoMapper(typeof(KundeProfile));
builder.Services.AddAutoMapper(typeof(WarenkorbProfile));

var app = builder.Build();
Startup.Configure(app);

app.Run();

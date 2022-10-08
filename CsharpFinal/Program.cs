using System.Runtime;
using CsharpFinal;

GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;


var files = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var endpoint = new FileEndpoint(files).Initialize();


WebApplicationOptions options = new()
{
    ContentRootPath = files
};

var builder = WebApplication.CreateBuilder(options);
if (builder.Environment.IsProduction())
{
    builder.Logging.ClearProviders();
    builder.WebHost.UseKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP(5010);
            serverOptions.AddServerHeader = false;
        }
    );
}
else
{
    builder.Logging.AddConsole().AddDebug();
}

var app = builder.Build();
app.Use((context, next) => endpoint.TryFetchFile(context, next));
app.UseRouting();
app.Run();
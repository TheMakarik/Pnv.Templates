using Microsoft.Extensions.Options;
using Pnv.Web.Application;
using Pnv.Web.Endpoints;
using Pnv.Web.Infrastructure;
using Pnv.Web.Options;
#if (UseLogging)
using Serilog;
#endif

var builder = WebApplication.CreateBuilder(args);

#if (UseLogging)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders().AddSerilog(Log.Logger, dispose: true);
#endif

builder.Services
    .AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection(nameof(AppOptions)));

builder.Services.AddSingleton<IGreetingService, GreetingService>();

#if (UseEfCore)
builder.Services.AddInfrastructure(builder.Configuration);
#endif

var app = builder.Build();

#if (UseEfCore)
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
}
#endif

app.MapGet("/", (IOptions<AppOptions> options) => TypedResults.Ok(new { options.Value.Title }));
app.MapGreetingEndpoints();

app.Run();

using Microsoft.Extensions.Options;
using Pnv.Combo.Web.Application;
using Pnv.Combo.Web.Endpoints;
using Pnv.Combo.Web.Infrastructure;
using Pnv.Combo.Web.Options;
#if (UseWebLogging)
using Serilog;
#endif

var builder = WebApplication.CreateBuilder(args);

#if (UseWebLogging)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders().AddSerilog(Log.Logger, dispose: true);
#endif

builder.Services
    .AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection(nameof(AppOptions)));

#if (UseBackendScrutor)
builder.Services.Scan(scan => scan
    .FromAssemblies(typeof(IGreetingService).Assembly, typeof(GreetingService).Assembly)
    .AddClasses(classes => classes.AssignableTo<IGreetingService>())
    .AsImplementedInterfaces()
    .WithSingletonLifetime());
#else
builder.Services.AddSingleton<IGreetingService, GreetingService>();
#endif

#if (UseWebEfCore)
builder.Services.AddInfrastructure(builder.Configuration);
#endif

var app = builder.Build();

#if (UseWebEfCore)
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
}
#endif

app.MapGet("/", (IOptions<AppOptions> options) => TypedResults.Ok(new { options.Value.Title }));
app.MapGreetingEndpoints();

app.Run();

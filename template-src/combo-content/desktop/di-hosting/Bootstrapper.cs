using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pnv.Combo.Desktop.Options;
using Pnv.Combo.Desktop.Services;
using Pnv.Combo.Desktop.ViewModels;
#if (ApiClient != "None")
using Pnv.Combo.Desktop.Api;
#endif
#if (UseDesktopLogging)
using Serilog;
#endif
#if (UseDesktopEfCore)
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pnv.Combo.Desktop.Infrastructure;
using Pnv.Combo.Desktop.Infrastructure.Interceptors;
#endif
#if (UseScrutor)
using Scrutor;
#endif

namespace Pnv.Combo.Desktop;

public sealed class Bootstrapper
{
    public IHost BuildHost()
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = Environment.GetCommandLineArgs(),
            ContentRootPath = AppContext.BaseDirectory,
        });

        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

#if (UseDesktopLogging)
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Logging.ClearProviders().AddSerilog(Log.Logger, dispose: true);
#endif

        RegisterOptions(builder);
        RegisterServices(builder);

        var host = builder.Build();

#if (UseDesktopEfCore)
        host.Services.GetRequiredService<DatabaseInitializer>().Initialize();
#endif

        return host;
    }

    private static void RegisterOptions(HostApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<AppOptions>()
            .Bind(builder.Configuration.GetSection(nameof(AppOptions)));
#if (UseDesktopEfCore)
        builder.Services
            .AddOptions<DatabaseOptions>()
            .Bind(builder.Configuration.GetSection(nameof(DatabaseOptions)));
#endif
    }

    private static void RegisterServices(HostApplicationBuilder builder)
    {
        var services = builder.Services;

#if (UseScrutor)
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(Bootstrapper), typeof(GreetingService))
            .AddClasses(classes => classes.AssignableTo<ViewModelBase>())
            .AsSelf()
            .WithSingletonLifetime()
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("Service", StringComparison.Ordinal)))
            .AsImplementedInterfaces()
            .WithSingletonLifetime());
#else
        services.AddSingleton<IGreetingService, GreetingService>();
        services.AddSingleton<MainViewModel>();
#endif
#if (ApiClient != "None")
        services.AddHttpClient<IGreetingsApiClient, GreetingsApiClient>((_, client) =>
            client.BaseAddress = GetApiBaseUrl(builder.Configuration));
#endif
#if (UseDesktopEfCore)
        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

            options
                .UseSqlite(GetConnectionString(databaseOptions))
                .AddInterceptors(new SqlLoggingInterceptor(
                    provider.GetRequiredService<ILogger<SqlLoggingInterceptor>>()));
        });

        services.AddSingleton<DatabaseInitializer>();
#endif
    }

#if (ApiClient != "None")
    private static Uri GetApiBaseUrl(IConfiguration configuration)
        => new(configuration["Api:BaseUrl"] ?? "http://localhost:5080/");
#endif

#if (UseDesktopEfCore)

    private static string GetConnectionString(DatabaseOptions options)
        => string.IsNullOrWhiteSpace(options.ConnectionString)
            ? $"Data Source={Path.Join(AppContext.BaseDirectory, "app.db")}"
            : options.ConnectionString;
#endif
}

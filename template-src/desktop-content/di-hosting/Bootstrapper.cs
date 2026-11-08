using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnvApp.Options;
using PnvApp.Services;
using PnvApp.ViewModels;
#if (UseLogging)
using Serilog;
#endif
#if (UseEfCore)
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PnvApp.Infrastructure;
using PnvApp.Infrastructure.Interceptors;
#endif
#if (UseScrutor)
using Scrutor;
#endif

namespace PnvApp;

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

#if (UseLogging)
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Logging.ClearProviders().AddSerilog(Log.Logger, dispose: true);
#endif

        RegisterOptions(builder);
        RegisterServices(builder.Services);

        var host = builder.Build();

#if (UseEfCore)
        host.Services.GetRequiredService<DatabaseInitializer>().Initialize();
#endif

        return host;
    }

    private static void RegisterOptions(HostApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<AppOptions>()
            .Bind(builder.Configuration.GetSection(nameof(AppOptions)));
#if (UseEfCore)
        builder.Services
            .AddOptions<DatabaseOptions>()
            .Bind(builder.Configuration.GetSection(nameof(DatabaseOptions)));
#endif
    }

    private static void RegisterServices(IServiceCollection services)
    {
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
#if (UseEfCore)
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
#if (UseEfCore)

    private static string GetConnectionString(DatabaseOptions options)
        => string.IsNullOrWhiteSpace(options.ConnectionString)
            ? $"Data Source={Path.Join(AppContext.BaseDirectory, "app.db")}"
            : options.ConnectionString;
#endif
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pnv.Combo.Web.Infrastructure.Interceptors;

namespace Pnv.Combo.Web.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            options
                .UseNpgsql(configuration.GetConnectionString("Default"))
                .AddInterceptors(new SqlLoggingInterceptor(
                    provider.GetRequiredService<ILogger<SqlLoggingInterceptor>>()));
        });

        services.AddSingleton<DatabaseInitializer>();
        return services;
    }
}

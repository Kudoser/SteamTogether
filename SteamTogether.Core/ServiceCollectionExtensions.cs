using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SteamTogether.Core.Context;
using SteamTogether.Core.Options;

namespace SteamTogether.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterDataServices(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(
            (provider, dbOptions) =>
            {
                var opts = provider.GetRequiredService<IOptions<DatabaseOptions>>();
                dbOptions.UseSqlite(opts.Value.ConnectionString);
            }
        );

        return services;
    }
}

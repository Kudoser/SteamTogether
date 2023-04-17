using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SteamTogether.Bot.Services;
using SteamTogether.Core.Context;

namespace SteamTogether.Bot;

public sealed class TelegramPollingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public TelegramPollingWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TelegramPollingWorker>>();
        logger.LogInformation(
            "SteamTogether.Bot v{Version}",
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes<AssemblyInformationalVersionAttribute>()
                .First()
                .InformationalVersion
        );

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Checking pending migrations");

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Running pending migrations");
            await context.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            logger.LogInformation("Database is up to date");
        }

        var telegram = scope.ServiceProvider.GetRequiredService<ITelegramService>();
        while (!cancellationToken.IsCancellationRequested)
        {
            await telegram.StartReceivingAsync(cancellationToken);
        }
    }
}

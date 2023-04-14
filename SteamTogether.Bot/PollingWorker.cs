using Microsoft.EntityFrameworkCore;
using SteamTogether.Bot.Services;
using SteamTogether.Core.Context;

namespace SteamTogether.Bot;

public sealed class PollingWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public PollingWorker(IServiceProvider serviceProvider, ILogger<PollingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PollingWorker>>();
        
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
        await telegram.StartReceivingAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping application...");

        return Task.CompletedTask;
    }
}

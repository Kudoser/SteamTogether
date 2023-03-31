using SteamTogether.Bot.Services;

namespace SteamTogether.Bot;

public sealed class PollingWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public PollingWorker(
        IServiceProvider serviceProvider,
        ILogger<PollingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var telegram = scope.ServiceProvider.GetRequiredService<ITelegramService>();
            
        await telegram.StartReceivingAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping application...");

        return Task.CompletedTask;
    }
}
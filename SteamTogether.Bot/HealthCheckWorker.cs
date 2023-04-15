using SteamTogether.Bot.Services;

namespace SteamTogether.Bot;

public class HealthCheckWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HealthCheckWorker> _logger;

    public HealthCheckWorker(IServiceProvider serviceProvider, ILogger<HealthCheckWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var healthCheckService = scope.ServiceProvider.GetRequiredService<IHealthCheckService>();

        await healthCheckService.StartAsync();
        while (!cancellationToken.IsCancellationRequested)
        {
            await healthCheckService.ReceiveAsync();
        }
    }
}
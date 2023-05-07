using Cronos;
using Microsoft.Extensions.Options;
using SteamTogether.Core.Services;
using SteamTogether.Scraper.Options;
using SteamTogether.Scraper.Services;
using System.Reflection;

namespace SteamTogether.Scraper;

public class ScraperWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScraperWorker> _logger;
    private DateTime? _nextSync;

    public ScraperWorker(
        IServiceProvider serviceProvider,
        ILogger<ScraperWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _logger.LogInformation(
            "{Name} v{Version}",
            Assembly.GetExecutingAssembly().GetName().Name,
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes<AssemblyInformationalVersionAttribute>()
                .First()
                .InformationalVersion
        );
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var scraper = scope.ServiceProvider.GetRequiredService<IScraperService>();
        var dateTimeService = scope.ServiceProvider.GetRequiredService<IDateTimeService>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<ScraperOptions>>().Value;
        var httpListener = scope.ServiceProvider.GetRequiredService<IHttpCommandListener>();
        
        await httpListener.StartAsync();
        WaitForHttpListenerAsync(httpListener, scraper, ct);
        
        if (options.RunOnStartup)
        {
            await scraper.RunSync();
        }
        
        _logger.LogInformation("Using schedule: {Schedule}", options.Schedule);
        var cron = CronExpression.Parse(options.Schedule, CronFormat.IncludeSeconds);
        _nextSync = cron.GetNextOccurrence(dateTimeService.UtcNow);
        
        while (!ct.IsCancellationRequested)
        {
            var utcNow = dateTimeService.UtcNow;
            var utcNext = cron.GetNextOccurrence(utcNow);

            if (utcNext == null)
            {
                _logger.LogWarning("No next run found, stopping worker");
                break;
            }

            var delay = utcNext.Value - utcNow;
            _logger.LogInformation("Next worker run: {Next} (in {Delay})", utcNext.Value, delay);

            await Task.Delay(delay, ct);
            await scraper.RunSync();
        }
    }

    private async Task WaitForHttpListenerAsync(IHttpCommandListener httpListener, IScraperService scraper, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await httpListener.ReceiveAsync(scraper);
        }
    }
}

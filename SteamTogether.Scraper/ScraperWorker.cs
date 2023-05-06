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
        
        if (options.RunOnStartup)
        {
            await scraper.RunSync();
        }

        await httpListener.StartAsync();
        
        _logger.LogInformation("Using schedule: {Schedule}", options.Schedule);
        var cron = CronExpression.Parse(options.Schedule, CronFormat.IncludeSeconds);
        _nextSync = cron.GetNextOccurrence(dateTimeService.UtcNow);
        while (!ct.IsCancellationRequested)
        {
            httpListener.ReceiveAsync(scraper);
            WaitForScraperNextRun(scraper, dateTimeService, cron, ct);
        }
    }

    private async Task WaitForScraperNextRun(IScraperService scraper, IDateTimeService dateTimeService, CronExpression cron, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_nextSync);
        
        var utcNow = dateTimeService.UtcNow;
        if (_nextSync <= dateTimeService.UtcNow)
        {
            _nextSync = cron.GetNextOccurrence(utcNow);
            var delay = _nextSync.Value - utcNow;
            
            _logger.LogInformation("Next worker run: {Next} (in {Delay})", _nextSync.Value, delay);
            scraper.RunSync();
        }
    }
}

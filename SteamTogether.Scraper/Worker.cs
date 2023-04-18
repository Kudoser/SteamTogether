using Cronos;
using Microsoft.Extensions.Options;
using NCrontab;
using SteamTogether.Core.Services;
using SteamTogether.Scraper.Options;
using SteamTogether.Scraper.Services;
using System.Reflection;

namespace SteamTogether.Scraper;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceProvider serviceProvider,
        ILogger<Worker> logger)
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var services = _serviceProvider.CreateScope().ServiceProvider;
        var scraper = services.GetRequiredService<IScrapperService>();
        var dateTimeService = services.GetRequiredService<IDateTimeService>();
        var options = services.GetRequiredService<IOptions<ScraperOptions>>().Value;
        
        if (options.RunOnStartup)
        {
            await scraper.RunSync();
        }

        _logger.LogInformation("Using schedule: {Schedule}", options.Schedule);
        var cron = CronExpression.Parse(options.Schedule, CronFormat.IncludeSeconds);
        while (!stoppingToken.IsCancellationRequested)
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

            await Task.Delay(delay, stoppingToken);

            await scraper.RunSync();
        }
    }
}

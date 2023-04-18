using Cronos;
using Microsoft.Extensions.Options;
using SteamTogether.Core.Services;
using SteamTogether.Scraper.Options;
using SteamTogether.Scraper.Services;
using System.Reflection;

namespace SteamTogether.Scraper;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;
    private readonly IDateTimeService _dateTimeService;

    public Worker(
        IDateTimeService dateTimeService,
        ILogger<Worker> logger,
        IServiceProvider serviceProvider
    )
    {
        _serviceProvider = serviceProvider.CreateScope().ServiceProvider;
        _dateTimeService = dateTimeService;
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
        var options = _serviceProvider.GetRequiredService<IOptions<ScraperOptions>>();
        var opts = options.Value;

        var scraper = _serviceProvider.GetRequiredService<IScrapperService>();
        if (opts.RunOnStartup)
        {
            await scraper.RunSync();
        }

        _logger.LogInformation("Using schedule: {Schedule}", opts.Schedule);
        var cron = CronExpression.Parse(opts.Schedule);
        while (!stoppingToken.IsCancellationRequested)
        {
            var utcNow = _dateTimeService.UtcNow;
            var utcNext = cron.GetNextOccurrence(utcNow);

            if (utcNext == null)
            {
                _logger.LogWarning("No next run found, stopping worker");
                break;
            }

            var delay = utcNext.Value - utcNow;
            _logger.LogInformation("Next worker run: {Next} (in {Delay})", utcNext.Value, delay);

            await Task.Delay(utcNext.Value - utcNow, stoppingToken);

            await scraper.RunSync();
        }
    }
}

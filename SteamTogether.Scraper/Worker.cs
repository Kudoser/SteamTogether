using Cronos;
using Microsoft.Extensions.Options;
using SteamTogether.Scraper.Options;
using SteamTogether.Scraper.Services;

namespace SteamTogether.Scraper;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
    {
        _serviceProvider = serviceProvider.CreateScope().ServiceProvider;
        _logger = logger;
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

        var cron = CronExpression.Parse(opts.Schedule);
        while (!stoppingToken.IsCancellationRequested)
        {
            var utcNow = DateTime.UtcNow;
            var utcNext = cron.GetNextOccurrence(utcNow);

            if (utcNext == null)
            {
                _logger.LogWarning("No next run found, stopping worker");
                break;
            }

            _logger.LogInformation("Next worker run: {Next}", utcNext);

            await Task.Delay(utcNext.Value - utcNow, stoppingToken);

            await scraper.RunSync();
        }
    }
}

using Cronos;
using Microsoft.Extensions.Options;
using SteamTogether.Core.Services;
using SteamTogether.Scraper.Options;
using SteamTogether.Scraper.Services;

namespace SteamTogether.Scraper;

public class Worker : BackgroundService
{
    private readonly IScrapperService _scraper;
    private readonly IDateTimeService _dateTimeService;
    private readonly ScraperOptions _options;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IScrapperService scrapper,
        IDateTimeService dateTimeService,
        IOptions<ScraperOptions> options,
        ILogger<Worker> logger)
    {
        _scraper = scrapper;
        _dateTimeService = dateTimeService;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.RunOnStartup)
        {
            await _scraper.RunSync();
        }

        _logger.LogInformation("Using schedule: {Schedule}", _options.Schedule);
        var cron = CronExpression.Parse(_options.Schedule);
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

            await _scraper.RunSync();
        }
    }
}

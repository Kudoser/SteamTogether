using Microsoft.Extensions.Options;
using SteamTogether.Core.Services;
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

        var schedule = NCrontab.CrontabSchedule.Parse(opts.Schedule);
        _logger.LogInformation("Worker running at: {Schedule}", opts.Schedule);
        while (!stoppingToken.IsCancellationRequested)
        {
            var dateTimeService = _serviceProvider.GetRequiredService<IDateTimeService>();
            var now = dateTimeService.GetCurrentTime();

            var nextExecutionTime = schedule.GetNextOccurrence(dateTimeService.GetCurrentTime());

            var period = nextExecutionTime - now;
            _logger.LogInformation(
                "Periodic timer debug: {Next} - {Now} = {Period}",
                nextExecutionTime,
                now,
                period
            );

            using var timer = new PeriodicTimer(nextExecutionTime - now);

            await timer.WaitForNextTickAsync(stoppingToken);

            await scraper.RunSync();
        }
    }
}

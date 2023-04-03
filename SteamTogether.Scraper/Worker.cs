using Microsoft.Extensions.Options;
using SteamTogether.Bot.Services;
using SteamTogether.Scraper.Options;

namespace SteamTogether.Scraper;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = _serviceProvider.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<ScraperOptions>>();
        var schedule = NCrontab.CrontabSchedule.Parse(options.Value.Schedule);

        while (!stoppingToken.IsCancellationRequested)
        {
            var dateTimeService = scope.ServiceProvider.GetRequiredService<IDateTimeService>();
            var now = dateTimeService.GetCurrentTime();

            var nextExecutionTime = schedule.GetNextOccurrence(dateTimeService.GetCurrentTime());
            using var timer = new PeriodicTimer(nextExecutionTime - now);

            _logger.LogInformation("Next worker run: {Next}", nextExecutionTime);
            await timer.WaitForNextTickAsync(stoppingToken);

            // @todo the job

            _logger.LogInformation("Processing job...done");
        }
    }
}

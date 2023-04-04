using Microsoft.Extensions.Options;
using SteamTogether.Bot.Services;
using SteamTogether.Core.Context;
using SteamTogether.Core.Services.Steam;
using SteamTogether.Scraper.Options;

namespace SteamTogether.Scraper.Services;

public class ScrapperService : IScrapperService
{
    private readonly ISteamService _steamService;
    private readonly ScraperOptions _options;
    private readonly ApplicationDbContext _dbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<ScrapperService> _logger;

    public ScrapperService(
        ISteamService steamService,
        IOptions<ScraperOptions> options,
        ApplicationDbContext dbContext,
        IDateTimeService dateTimeService,
        ILogger<ScrapperService> logger
    )
    {
        _steamService = steamService;
        _options = options.Value;
        _dbContext = dbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task RunSync()
    {
        _logger.LogInformation("Starting sync...");

        var syncDate = _dateTimeService.GetCurrentTime().AddSeconds(-_options.SyncPeriodSeconds);
        var steamPlayers = _dbContext.SteamPlayers
            .Where(p => p.LastSyncDateTime == null || p.LastSyncDateTime < syncDate)
            .Take(_options.PlayersPerRun)
            .ToArray();

        if (!steamPlayers.Any())
        {
            _logger.LogInformation("Nothing to process");
            return;
        }

        // @todo fetch online/cooperative games
        // @todo save to database
        var summaries = await _steamService
            .GetSteamUserWebInterface()
            .GetPlayerSummariesAsync(steamPlayers.Select(p => p.PlayerId).ToArray());

        _logger.LogInformation("Done");

        return;
    }
}

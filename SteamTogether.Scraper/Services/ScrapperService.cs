using SteamTogether.Bot.Services;
using SteamTogether.Core.Context;
using SteamTogether.Core.Services.Steam;

namespace SteamTogether.Scraper.Services;

public class ScrapperService : IScrapperService
{
    private readonly ISteamService _steamService;
    private readonly ApplicationDbContext _dbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<ScrapperService> _logger;

    public ScrapperService(
        ISteamService steamService,
        ApplicationDbContext dbContext,
        IDateTimeService dateTimeService,
        ILogger<ScrapperService> logger
    )
    {
        _steamService = steamService;
        _dbContext = dbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task RunSync()
    {
        _logger.LogInformation("Starting sync...");

        var steamPlayers = _dbContext.SteamPlayers
            .Where(
                p =>
                    p.LastSyncDateTime == null
                    || p.LastSyncDateTime < _dateTimeService.GetCurrentTime().AddHours(-5)
            )
            .Take(10)
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

        return;
    }
}

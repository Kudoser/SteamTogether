using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Steam.Models.SteamStore;
using SteamTogether.Core.Context;
using SteamTogether.Core.Exceptions;
using SteamTogether.Core.Models;
using SteamTogether.Core.Services;
using SteamTogether.Core.Services.Steam;
using SteamTogether.Scraper.Options;
using SteamWebAPI2.Interfaces;

namespace SteamTogether.Scraper.Services;

public class ScrapperService : IScrapperService
{
    private readonly ScraperOptions _options;
    private readonly ApplicationDbContext _dbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<ScrapperService> _logger;
    private readonly PlayerService _steamUserService;
    private readonly SteamStore _steamStoreService;

    public ScrapperService(
        ISteamService steamService,
        IOptions<ScraperOptions> options,
        ApplicationDbContext dbContext,
        IDateTimeService dateTimeService,
        ILogger<ScrapperService> logger
    )
    {
        _options = options.Value;
        _dbContext = dbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;

        _steamUserService = steamService.GetSteamUserWebInterface<PlayerService>();
        _steamStoreService = steamService.CreateSteamStoreInterface();
    }

    public async Task RunSync()
    {
        _logger.LogInformation("Starting sync...");

        var syncDate = _dateTimeService.GetCurrentTime().AddSeconds(-_options.PlayerSyncPeriodSeconds);
        var steamPlayers = _dbContext.SteamPlayers
            .Where(p => p.LastSyncDateTime == null || p.LastSyncDateTime < syncDate)
            .Include(player => player.Games)
            .Take(_options.PlayersPerRun)
            .ToArray();

        if (!steamPlayers.Any())
        {
            _logger.LogInformation("Nothing to process");
            return;
        }

        foreach (var player in steamPlayers)
        {
            var ownedGamesRequest = await _steamUserService
                .GetOwnedGamesAsync(player.PlayerId);

            var ownedGameIds = ownedGamesRequest.Data.OwnedGames
                .Select(o => o.AppId)
                .ToArray();

            await RemoveDisconnectedGamesFromPlayer(player, ownedGameIds);

            var ownedGames = _dbContext.SteamGames
                .Where(g => ownedGameIds.Contains(g.GameId))
                .ToArray();

            foreach (var ownedGameId in ownedGameIds)
            {
                _logger.LogInformation("Start sync for game Id={GameId}", ownedGameId);

                var lastGamesSync = _dateTimeService.GetCurrentTime().AddMinutes(-_options.GamesSyncPeriodMinutes);
                var game = ownedGames.FirstOrDefault(g => g.GameId == ownedGameId);
                if (game?.LastSyncDateTime == null || game.LastSyncDateTime < lastGamesSync)
                {
                    StoreAppDetailsDataModel storeApp;
                    try
                    {
                        storeApp = await _steamStoreService.GetStoreAppDetailsAsync(ownedGameId);
                    }
                    catch (Exception)
                    {
                        _logger.LogError("App {AppId} doesn't exist", ownedGameId);
                        continue;
                    }

                    var multiplayer = storeApp.Categories.Any(
                        // @todo move constants
                        category => new uint[] {1, 9, 38}.Contains(category.Id)
                    );

                    if (game == null)
                    {
                        game = new SteamGame
                        {
                            GameId = ownedGameId,
                            SteamAppId = storeApp.SteamAppId,
                            Name = storeApp.Name,
                            Multiplayer = multiplayer
                        };

                        _logger.LogInformation("Adding GameId={GameId}, Name={Name}", ownedGameId, game.Name);
                        _dbContext.SteamGames.Add(game);
                    }
                    else
                    {
                        game.SteamAppId = storeApp.SteamAppId;
                        game.Name = storeApp.Name;
                        game.Multiplayer = multiplayer;

                        _logger.LogInformation("Updating GameId={GameId}, Name={Name}", ownedGameId, game.Name);
                        _dbContext.SteamGames.Update(game);
                    }
                }
                else
                {
                    _logger.LogInformation("Up to date, last synced at {LastUpdated}", game.LastSyncDateTime);
                }
                
                game.LastSyncDateTime = _dateTimeService.GetCurrentTime();

                var connected = player.Games
                    .Select(g => g.GameId)
                    .Contains(game.GameId);

                if (!connected)
                {
                    _logger.LogInformation("Adding GameId={GameId} to player {Name}", ownedGameId, player.Name);
                    player.Games.Add(game);
                }
                
                player.LastSyncDateTime = _dateTimeService.GetCurrentTime();
            }

            var count = await _dbContext.SaveChangesAsync();
            _logger.LogInformation("{Count} games for player {Name} were synced", count, player.Name);
        }

        _logger.LogInformation("Done");
    }

    private async Task RemoveDisconnectedGamesFromPlayer(SteamPlayer player, uint[] ownedGameIds)
    {
        var notOwnedGames = player.Games
            .Where(game => !ownedGameIds.Contains(game.GameId))
            .ToArray();

        if (notOwnedGames.Any())
        {
            _logger.LogInformation("Removing {Count} games from player {Name}", notOwnedGames.Length, player.Name);
            foreach (var notOwnedGame in notOwnedGames)
            {
                player.Games.Remove(notOwnedGame);
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task<SteamGame> GetSteamGame(uint gameId)
    {
        StoreAppDetailsDataModel storeApp;
        try
        {
            storeApp = await _steamStoreService.GetStoreAppDetailsAsync(gameId);
        }
        catch (Exception)
        {
            _logger.LogError("App {AppId} doesn't exist", gameId);
            throw;
        }

        var multiplayer = storeApp.Categories.Any(
            // @todo move constants
            category => new uint[] {1, 9, 38}.Contains(category.Id)
        );

        return new SteamGame
        {
            SteamAppId = storeApp.SteamAppId,
            Name = storeApp.Name,
            LastSyncDateTime = _dateTimeService.GetCurrentTime(),
            Multiplayer = multiplayer
        };
    }
}
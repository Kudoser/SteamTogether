using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Steam.Models.SteamCommunity;
using Steam.Models.SteamStore;
using SteamTogether.Core.Context;
using SteamTogether.Core.Models;
using SteamTogether.Core.Services;
using SteamTogether.Core.Services.Steam;
using SteamTogether.Scraper.Options;

namespace SteamTogether.Scraper.Services;

public class ScraperService : IScraperService
{
    private readonly ScraperOptions _options;
    private readonly ISteamService _steamService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<ScraperService> _logger;
    private readonly ApplicationDbContext _dbContext;
    
    public ScraperSyncStatus SyncStatus { get; set; }
    private List<SteamGame> _allGames = new();
    private List<SteamGameCategory> _allGameCategories = new();

    public ScraperService(
        ISteamService steamService,
        IOptions<ScraperOptions> options,
        ApplicationDbContext dbContext,
        IDateTimeService dateTimeService,
        ILogger<ScraperService> logger)
    {
        _options = options.Value;
        _steamService = steamService;
        _dbContext = dbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
        SyncStatus = ScraperSyncStatus.Waiting;
    }

    public async Task RunSync()
    {
        await RunSync(Array.Empty<ulong>());
    }

    public async Task RunSync(ulong[] playerIds)
    {
        if (SyncStatus == ScraperSyncStatus.InProgress)
        {
            _logger.LogInformation("Sync in process...");
            return;
        }
        
        SyncStatus = ScraperSyncStatus.InProgress;
        try
        {
            SteamPlayer[] steamPlayers;
            if (playerIds.Any())
            {
                steamPlayers = _dbContext.SteamPlayers
                    .Where(p => playerIds.Contains(p.PlayerId))
                    .Include(player => player.PlayerGames)
                    .ThenInclude(p => p.Game)
                    .ToArray();
            }
            else
            {
                var syncDate = _dateTimeService.UtcNow.AddSeconds(-_options.PlayerSyncPeriodSeconds);
                steamPlayers = _dbContext.SteamPlayers
                    .Where(p => p.LastSyncDateTime == null || p.LastSyncDateTime < syncDate)
                    .Include(player => player.PlayerGames)
                    .ThenInclude(p => p.Game)
                    .Take(_options.PlayersPerRun)
                    .ToArray();
            }

            if (!steamPlayers.Any())
            {
                _logger.LogInformation("Nothing to process");
                SyncStatus = ScraperSyncStatus.Waiting;
                return;
            }

            _allGames = _dbContext.SteamGames
                .Include(g => g.Categories)
                .ToList();

            _allGameCategories = _dbContext.SteamGamesCategories
                .ToList();

            _logger.LogInformation("Starting sync for {Count} players...", steamPlayers.Length);
            foreach (var player in steamPlayers)
            {
                await SyncPlayerAsync(player);
            }

            var count = await _dbContext.SaveChangesAsync();
            _logger.LogInformation("{Count} items over all were saved", count);
            _logger.LogInformation("Done");
        }
        catch
        {
            CleanUp();
            SyncStatus = ScraperSyncStatus.Error;
            throw;
        }

        CleanUp();
        SyncStatus = ScraperSyncStatus.Waiting;
    }

    private async Task SyncPlayerAsync(SteamPlayer player)
    {
        _logger.LogInformation("Processing player Name={Name}", player.Name);
        var ownedGamesRequest = await _steamService.GetOwnedGamesAsync(player.PlayerId);
        
        foreach (var ownedGame in ownedGamesRequest.Data.OwnedGames)
        {
            await SyncGameAsync(player, ownedGame);
        }

        player.LastSyncDateTime = _dateTimeService.UtcNow;
    }

    private async Task SyncGameAsync(SteamPlayer player, OwnedGameModel ownedGame)
    {
        _logger.LogInformation("Start sync for game Id={GameId}", ownedGame.AppId);
        
        var game = await InsertOrUpdateGameAsync(ownedGame.AppId);
        if (game == null)
        {
            return;
        }

        var connected = player.PlayerGames.FirstOrDefault(g => g.GameId == game.GameId);
        if (connected == null)
        {
            _logger.LogInformation(
                "Adding GameId={GameId} to player {Name}",
                ownedGame.AppId,
                player.Name
            );
            var playerGameRelation = new PlayerGame
            {
                GameId = game.GameId,
                PlayerId = player.PlayerId,
                PlaytimeForever = ownedGame.PlaytimeForever,
                PlaytimeLastTwoWeeks = ownedGame.PlaytimeForever
            };
            
            player.PlayerGames.Add(playerGameRelation);
        }
    }

    private async Task<SteamGame?> InsertOrUpdateGameAsync(uint gameId)
    {
        var game = _allGames.FirstOrDefault(g => g.GameId == gameId);
        
        var lastGamesSync = _dateTimeService.UtcNow.AddMinutes(-_options.GamesSyncPeriodMinutes);
        if (game?.LastSyncDateTime == null || game.LastSyncDateTime < lastGamesSync)
        {
            StoreAppDetailsDataModel storeApp;
            try
            {
                storeApp = await _steamService.GetAppDetailsAsync(gameId);
            }
            catch (Exception)
            {
                _logger.LogWarning("App {AppId} doesn't exist", gameId);
                return null;
            }

            if (game == null)
            {
                game = new SteamGame
                {
                    GameId = gameId,
                    SteamAppId = storeApp.SteamAppId,
                    Name = storeApp.Name
                };

                _logger.LogInformation(
                    "Adding GameId={GameId}, Name={Name}",
                    gameId,
                    game.Name
                );
                _allGames.Add(game);
                _dbContext.SteamGames.Add(game);
            }
            else
            {
                game.SteamAppId = storeApp.SteamAppId;
                game.Name = storeApp.Name;

                _logger.LogInformation(
                    "Updating GameId={GameId}, Name={Name}",
                    gameId,
                    game.Name
                );
            }

            SyncCategories(game, storeApp);
            
            game.LastSyncDateTime = _dateTimeService.UtcNow;
        }
        
        return game;
    }

    private void SyncCategories(SteamGame game, StoreAppDetailsDataModel storeApp)
    {
        foreach (var storeCategory in storeApp.Categories)
        {
            var gameCategory = _allGameCategories.FirstOrDefault(c => c.CategoryId == storeCategory.Id);
            if (gameCategory == null)
            {
                gameCategory = new SteamGameCategory
                {
                    CategoryId = storeCategory.Id,
                    Description = storeCategory.Description
                };

                _logger.LogInformation("New category: {NewCategory}", storeCategory.Description);
                _allGameCategories.Add(gameCategory);
                _dbContext.SteamGamesCategories.Add(gameCategory);
            }
            else
            {
                gameCategory.Description = storeCategory.Description;
            }
            
            var connected = game.Categories.Select(c => c.CategoryId).Contains(gameCategory.CategoryId);
            if (!connected)
            {
                game.Categories.Add(gameCategory);
            }
        }
    }

    private void CleanUp()
    {
        _allGames.Clear();
        _allGameCategories.Clear();
    }
}
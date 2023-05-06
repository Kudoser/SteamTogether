using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Steam.Models.SteamStore;
using SteamTogether.Core.Context;
using SteamTogether.Core.Models;
using SteamTogether.Core.Services;
using SteamTogether.Core.Services.Steam;
using SteamTogether.Scraper.Options;

namespace SteamTogether.Scraper.Services;

public class ScrapperService : IScrapperService
{
    private readonly ScraperOptions _options;
    private readonly ISteamService _steamService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<ScrapperService> _logger;
    private readonly ApplicationDbContext _dbContext;
    
    private List<SteamGame> _allGames = new();
    private List<SteamGameCategory> _allGameCategories = new();

    public ScrapperService(
        ISteamService steamService,
        IOptions<ScraperOptions> options,
        ApplicationDbContext dbContext,
        IDateTimeService dateTimeService,
        ILogger<ScrapperService> logger
    )
    {
        _options = options.Value;
        _steamService = steamService;
        _dbContext = dbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task RunSync()
    {
        _logger.LogInformation("Starting sync...");
        var syncDate = _dateTimeService.UtcNow.AddSeconds(-_options.PlayerSyncPeriodSeconds);
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
        _allGames = _dbContext.SteamGames
            .Include(g => g.Categories)
            .ToList();

        _allGameCategories = _dbContext.SteamGamesCategories
            .ToList();
        
        foreach (var player in steamPlayers)
        {
            await SyncPlayerAsync(player);
        }

        var count = await _dbContext.SaveChangesAsync();
        _logger.LogInformation("{Count} items over all were saved", count);
        _logger.LogInformation("Done");
    }

    private async Task SyncPlayerAsync(SteamPlayer player)
    {
        _logger.LogInformation("Processing player Name={Name}", player.Name);
        var ownedGamesRequest = await _steamService.GetOwnedGamesAsync(player.PlayerId);
        var ownedGameIds = ownedGamesRequest.Data.OwnedGames
            .Select(o => o.AppId)
            .ToArray();
        
        foreach (var ownedGameId in ownedGameIds)
        {
            await SyncGameAsync(player, ownedGameId);
        }

        player.LastSyncDateTime = _dateTimeService.UtcNow;
    }

    private async Task SyncGameAsync(SteamPlayer player, uint ownedGameId)
    {
        _logger.LogInformation("Start sync for game Id={GameId}", ownedGameId);
        
        var game = await InsertOrUpdateGameAsync(ownedGameId);
        if (game == null)
        {
            return;
        }
        
        var connected = player.Games.Select(g => g.GameId).Contains(game.GameId);
        if (!connected)
        {
            _logger.LogInformation(
                "Adding GameId={GameId} to player {Name}",
                ownedGameId,
                player.Name
            );
            player.Games.Add(game);
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

            var multiplayer = storeApp.Categories.Any(
                // @todo move constants
                category => new uint[] {1, 9, 38}.Contains(category.Id)
            );

            if (game == null)
            {
                game = new SteamGame
                {
                    GameId = gameId,
                    SteamAppId = storeApp.SteamAppId,
                    Name = storeApp.Name,
                    Multiplayer = multiplayer
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
                game.Multiplayer = multiplayer;

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
}
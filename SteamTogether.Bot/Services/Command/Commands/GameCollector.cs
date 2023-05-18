using Microsoft.EntityFrameworkCore;
using SteamTogether.Bot.Models;
using SteamTogether.Core.Context;

namespace SteamTogether.Bot.Services.Command.Commands;

public class GameCollector
{
    private readonly ApplicationDbContext _dbContext;

    public GameCollector(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public GroupedGameResult[] GetGroupGames(long[] telegramUserIds, uint[] categoryIds, int amountOfGames)
    {
        var playerIds = _dbContext.TelegramChatParticipants
            .Where(p => telegramUserIds.Contains(p.TelegramUserId))
            .Select(cp => cp.SteamPlayerId)
            .ToArray();

        var results = new GroupedGameResult[] { };
        if (!playerIds.Any())
        {
            return results;
        }

        return _dbContext.PlayerGame
            .Where(pg => playerIds.Contains(pg.PlayerId))
            .Where(pg => pg.Game.Categories.Any(c => categoryIds.Contains(c.CategoryId)))
            .Include(pg => pg.Player)
            .Include(pg => pg.Game)
            .GroupBy(g => new {g.GameId, g.Game.Name})
            .Select(g => new GroupedGameResult
            {
                Name = g.Key.Name,
                Count = g.Count(),
                PlayerNames = g.Select(p => p.Player.Name).ToArray(),
                TotalSeconds = g.Select(g => g.PlaytimeForever.TotalSeconds).ToArray()
            })
            .AsEnumerable()
            .OrderByDescending(a => a.Count)
            .ThenByDescending(a => a.TotalSeconds.Average())
            .Take(amountOfGames)
            .ToArray();
    }
}
using Microsoft.EntityFrameworkCore;
using SteamTogether.Core.Context;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SteamTogether.Bot.Services.Command.Commands;

public class PlayCommand : ITelegramCommand
{
    public const string Name = "play";
    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PlayCommand> _logger;

    public PlayCommand(
        ITelegramBotClient telegramClient,
        ApplicationDbContext dbContext,
        ILogger<PlayCommand> logger
    )
    {
        _telegramClient = telegramClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(Message inputMessage, IEnumerable<string> args)
    {
        var chatId = inputMessage.Chat.Id;

        var chat = _dbContext.TelegramChat
            .Where(chat => chat.ChatId == chatId)
            .Include(chat => chat.Players)
            .ThenInclude(player => player.Games)
            .ThenInclude(game => game.Categories)
            .FirstOrDefault();

        ArgumentNullException.ThrowIfNull(chat);

        // @todo reconsider args and parameters
        // @todo default category
        uint categoryId = 9;
        var unparsedCategoryId = args.FirstOrDefault();
        if (unparsedCategoryId != null)
        {
            if (!uint.TryParse(unparsedCategoryId, out categoryId))
            {
                await SendMessage(chatId, $"Parse error");
                return;
            }
        }
        
        var category = _dbContext.SteamGamesCategories.FirstOrDefault(c => c.CategoryId == categoryId);
        if (category == null)
        {
            await SendMessage(chatId, $"Category {categoryId} doesn't exist");
            return;
        }
        

        var games = chat.Players
            .SelectMany(
                player => player.Games.Where(game => game.Categories.Any(c => c.CategoryId == category.CategoryId)),
                (player, game) =>
                    new
                    {
                        PlayerName = player.Name,
                        GameName = game.Name,
                        game.GameId
                    }
            )
            .GroupBy(p => new { p.GameId, p.GameName })
            .Select(
                g =>
                    new
                    {
                        Name = g.Key.GameName,
                        Count = g.Count(),
                        Players = string.Join(",", g.Select(p => p.PlayerName))
                    }
            )
            .OrderByDescending(x => x.Count)
            .Take(15);

        if (!games.Any())
        {
            await SendMessage(chatId, "No games found");
            return;
        }

        var messageLines = new List<string> {$"Category: {category.Description}({categoryId})"};
        var lines = games.Select(
            (g, i) => $"{i + 1}. {g.Name}, count: {g.Count} ({g.Players})"
        );
        messageLines.AddRange(lines);
        await SendMessage(chatId, string.Join("\n", messageLines));
    }

    private async Task SendMessage(long chatId, string message)
    {
        await _telegramClient.SendTextMessageAsync(
            parseMode: ParseMode.Html,
            chatId: chatId,
            text: message,
            cancellationToken: new CancellationToken()
        );
    }
}

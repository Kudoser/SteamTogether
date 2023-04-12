using Microsoft.EntityFrameworkCore;
using SteamTogether.Core.Context;
using SteamTogether.Core.Models;
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
            .FirstOrDefault();
        
        ArgumentNullException.ThrowIfNull(chat);

        var games = chat.Players.SelectMany(player => player.Games,
                (player, game) => new {PlayerName = player.Name, GameName = game.Name})
            .GroupBy(p => p.GameName)
            .Select(g =>
            new {
                Name = g.Key,
                Count = g.Count(),
                Players = string.Join(",", g.Select(p => p.PlayerName)) 
            })
            .OrderByDescending(x => x.Count)
            .Take(15);

        var messageLines = games
            .Select((g,i) => $"{i + 1}. {g.Name}, count: {g.Count} ({g.Players})");
        var message = string.Join("\n", messageLines);
        await SendMessage(chatId, message);
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
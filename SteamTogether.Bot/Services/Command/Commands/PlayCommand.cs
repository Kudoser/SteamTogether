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
            .Include(c => c.Players)
            .ThenInclude(player => player.Games.Where(game => game.Multiplayer))
            .FirstOrDefault();

        ArgumentNullException.ThrowIfNull(chat);
        
        var games = new List<SteamGame>();
        foreach (var player in chat.Players)
        {
            games.AddRange(player.Games);
        }

        var uniqueGames = games
            .DistinctBy(game => game.GameId)
            .ToList();

        if (uniqueGames.Count > 20)
        {
            await SendMessage(chatId, $"Game list is too long, it contains {uniqueGames.Count} games");
            return;
        }

        var message = string.Join("\n", uniqueGames.Select(game => game.Name));
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
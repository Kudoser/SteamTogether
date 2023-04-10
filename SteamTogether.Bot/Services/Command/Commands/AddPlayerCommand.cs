using Microsoft.EntityFrameworkCore;
using SteamTogether.Core.Context;
using SteamTogether.Core.Models;
using SteamTogether.Core.Services.Steam;
using SteamWebAPI2.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Command.Commands;

public class AddPlayerListCommand : ITelegramCommand
{
    public const string Name = "add";
    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ISteamService _steamService;
    private readonly ILogger<AddPlayerListCommand> _logger;

    public AddPlayerListCommand(
        ITelegramBotClient telegramClient,
        ApplicationDbContext dbContext,
        ISteamService steamService,
        ILogger<AddPlayerListCommand> logger
    )
    {
        _telegramClient = telegramClient;
        _dbContext = dbContext;
        _steamService = steamService;
        _logger = logger;
    }

    public async Task ExecuteAsync(Message inputMessage, IEnumerable<string> args)
    {
        var chatId = inputMessage.Chat.Id;
        var unparsedPlayerId = args.FirstOrDefault();
        if (string.IsNullOrEmpty(unparsedPlayerId))
        {
            await SendMessage(chatId, "argument can't be null");
            return;
        }

        if (!ulong.TryParse(unparsedPlayerId, out var playerId))
        {
            await SendMessage(chatId, "steamId should be a number");
            return;
        }

        var chat = _dbContext.TelegramChat
            .Where(chat => chat.ChatId == chatId)
            .Include(c => c.Players)
            .FirstOrDefault();

        if (chat == null)
        {
            chat = new TelegramChat { ChatId = chatId };
        }

        if (chat.Players.Select(p => p.PlayerId).Contains(playerId))
        {
            await SendMessage(chatId, $"player {unparsedPlayerId} has already been added");
            return;
        }

        var steamUserService = _steamService.GetSteamUserWebInterface<SteamUser>();
        var player = await steamUserService.GetPlayerSummaryAsync(playerId);
        if (player == null)
        {
            await SendMessage(chatId, $"player with ID={playerId} doesn't exist");
            return;
        }

        // @todo check if profile is public
        // @todo add possibility to read APIKEY

        chat.Players.Add(new SteamPlayer { PlayerId = player.Data.SteamId });

        _dbContext.TelegramChat.Add(chat);
        await _dbContext.SaveChangesAsync();

        await SendMessage(chatId, $"{player.Data.Nickname} has been added");
    }

    private async Task SendMessage(long chatId, string message)
    {
        await _telegramClient.SendTextMessageAsync(
            chatId: chatId,
            text: message,
            cancellationToken: new CancellationToken()
        );
    }
}

using Microsoft.EntityFrameworkCore;
using Steam.Models.SteamCommunity;
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
        
        var player = await _dbContext.SteamPlayers.FindAsync(playerId);
        if (player == null)
        {
            var steamUserService = _steamService.GetSteamUserWebInterface<SteamUser>();
            var steamWebResponse = await steamUserService.GetPlayerSummaryAsync(playerId);
            if (steamWebResponse == null)
            {
                await SendMessage(chatId, $"player with ID={playerId} doesn't exist");
                return;
            }

            // @todo add public API Key support
            if (steamWebResponse.Data.ProfileVisibility != ProfileVisibility.Public)
            {
                await SendMessage(chatId, $"Steam profile is not public");
                return;
            }

            player = new SteamPlayer
            {
                PlayerId = steamWebResponse.Data.SteamId,
                Name = steamWebResponse.Data.Nickname
            };
            _dbContext.SteamPlayers.Add(player);
        }

        var chat = _dbContext.TelegramChat
            .Where(chat => chat.ChatId == chatId)
            .Include(chat => chat.Players)
            .FirstOrDefault();
        
        if (chat == null)
        {
            chat = new TelegramChat {ChatId = chatId};
            _dbContext.TelegramChat.Add(chat);
        }
        
        if (chat.Players.FirstOrDefault(p => p.PlayerId == player.PlayerId) != null)
        {
            await SendMessage(chatId, $"player {player.Name} has already been added");
            return;
        }
        chat.Players.Add(player);
        await _dbContext.SaveChangesAsync();

        await SendMessage(chatId, $"{player.Name} has been added");
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

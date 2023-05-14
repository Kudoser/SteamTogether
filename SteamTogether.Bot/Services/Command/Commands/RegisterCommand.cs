using Microsoft.EntityFrameworkCore;
using Steam.Models.SteamCommunity;
using SteamTogether.Core.Context;
using SteamTogether.Core.Models;
using SteamTogether.Core.Services.Steam;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Command.Commands;

public class RegisterCommand : ITelegramCommand
{
    public const string Name = "register";
    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ISteamService _steamService;

    public RegisterCommand(
        ITelegramBotClient telegramClient,
        ApplicationDbContext dbContext,
        ISteamService steamService
    )
    {
        _telegramClient = telegramClient;
        _dbContext = dbContext;
        _steamService = steamService;
    }

    public async Task ExecuteAsync(Message inputMessage, string[] args)
    {
        var chatId = inputMessage.Chat.Id;
        var unparsedSteamPlayerId = args.FirstOrDefault();
        if (string.IsNullOrEmpty(unparsedSteamPlayerId))
        {
            await SendMessage(chatId, "argument can't be null");
            return;
        }

        if (!ulong.TryParse(unparsedSteamPlayerId, out var steamPlayerId))
        {
            await SendMessage(chatId, "steamId should be a number");
            return;
        }

        if (inputMessage.From == null || inputMessage.From.IsBot)
        {
            await SendMessage(chatId, "Unsupported message");
            return;
        }

        var fromUserId = inputMessage.From.Id;
        var chatUser = _dbContext.TelegramChatParticipants
            .Where(c => c.ChatId == chatId)
            .Include(c => c.SteamPlayer)
            .FirstOrDefault(c => c.TelegramUserId == fromUserId);

        if (chatUser != null)
        {
            await SendMessage(chatId,
                $"You have already registered SteamID={chatUser.SteamPlayer.PlayerId}({chatUser.SteamPlayer.Name})");
            return;
        }

        var chatPlayer = _dbContext.TelegramChatParticipants
            .Where(c => c.ChatId == chatId)
            .Include(c => c.SteamPlayer)
            .FirstOrDefault(c => c.SteamPlayerId == steamPlayerId);

        if (chatPlayer != null)
        {
            await SendMessage(chatId, "This steam ID have already been registered");
            return;
        }

        var steamWebResponse = await _steamService.GetPlayerSummaryAsync(steamPlayerId);
        if (steamWebResponse?.Data == null)
        {
            await SendMessage(chatId, $"player with ID={steamPlayerId} doesn't exist");
            return;
        }

        if (steamWebResponse.Data.ProfileVisibility != ProfileVisibility.Public)
        {
            await SendMessage(
                chatId,
                $"{steamWebResponse.Data.Nickname}'s steam profile is not public"
            );
            return;
        }

        var player = await _dbContext.SteamPlayers.FindAsync(steamPlayerId);
        if (player == null)
        {
            player = new SteamPlayer
            {
                PlayerId = steamPlayerId,
                Name = steamWebResponse.Data.Nickname
            };
            _dbContext.SteamPlayers.Add(player);
        }
        else
        {
            player.Name = steamWebResponse.Data.Nickname;
            _dbContext.SteamPlayers.Update(player);
        }

        var chatParticipant = new TelegramChatParticipant
        {
            ChatId = chatId,
            SteamPlayerId = steamPlayerId,
            TelegramUserId = fromUserId
        };

        _dbContext.TelegramChatParticipants.Add(chatParticipant);
        await _dbContext.SaveChangesAsync();

        await SendMessage(chatId, $"Steam account {player.Name} connected");
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
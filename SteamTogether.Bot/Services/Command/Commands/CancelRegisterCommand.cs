using Microsoft.EntityFrameworkCore;
using SteamTogether.Core.Context;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Command.Commands;

public class CancelRegisterCommand : ITelegramCommand
{
    public const string Name = "cancel";
    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;

    public CancelRegisterCommand(
        ITelegramBotClient telegramClient,
        ApplicationDbContext dbContext
    )
    {
        _telegramClient = telegramClient;
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(Message inputMessage, string[] args)
    {
        var chatId = inputMessage.Chat.Id;
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

        if (chatUser == null)
        {
            await SendMessage(chatId, "You have not registered yet");
            return;
        }
        
        _dbContext.TelegramChatParticipants.Remove(chatUser);
        await _dbContext.SaveChangesAsync();

        await SendMessage(chatId, "Steam account registration was canceled");
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
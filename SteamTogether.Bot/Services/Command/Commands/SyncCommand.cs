using Telegram.Bot;
using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Command.Commands;

public class SyncCommand : ITelegramCommand
{
    public const string Name = "sync";
    private readonly ITelegramBotClient _telegramClient;
    private readonly IScraperCommandClient _commandClient;

    public SyncCommand(
        ITelegramBotClient telegramClient,
        IScraperCommandClient commandClient
    )
    {
        _telegramClient = telegramClient;
        _commandClient = commandClient;
    }

    public async Task ExecuteAsync(Message inputMessage, string[] args)
    {
        var chatId = inputMessage.Chat.Id;

        var result = await _commandClient.RequestSyncAsync(args);
        var response = result.Success
            ? "Sync has started"
            : result.Message ?? "Error has occurred";
        
        await SendMessage(chatId, response);
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

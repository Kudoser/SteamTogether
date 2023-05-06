using Telegram.Bot;
using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Command.Commands;

public class SyncCommand : ITelegramCommand
{
    public const string Name = "sync";
    private readonly ITelegramBotClient _telegramClient;
    private readonly IScraperCommandClient _commandClient;
    private readonly ILogger<SyncCommand> _logger;

    public SyncCommand(
        ITelegramBotClient telegramClient,
        IScraperCommandClient commandClient,
        ILogger<SyncCommand> logger
    )
    {
        _telegramClient = telegramClient;
        _commandClient = commandClient;
        _logger = logger;
    }

    public async Task ExecuteAsync(Message inputMessage, string[] args)
    {
        var chatId = inputMessage.Chat.Id;

        var result = await _commandClient.RequestSync(args);
        
        await SendMessage(chatId, result);
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

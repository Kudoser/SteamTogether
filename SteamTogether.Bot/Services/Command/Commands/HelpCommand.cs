using Telegram.Bot;
using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Command.Commands;

public class HelpCommand : ITelegramCommand
{
    public const string Name = "help";
    private readonly ITelegramBotClient _telegramClient;

    public HelpCommand(ITelegramBotClient telegramClient)
    {
        _telegramClient = telegramClient;
    }

    public async Task ExecuteAsync(Message inputMessage, IEnumerable<string> args)
    {
        var chatId = inputMessage.Chat.Id;
        var help = @"
        How to:
        
        /list - returns players ready to play
        /add [SteamPlayerId:int] - register to play. Example: /add 123
        /play - provides a list of common multiplayer games";

        await SendMessage(chatId, help);
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
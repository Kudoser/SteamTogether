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

    public async Task ExecuteAsync(Message inputMessage, string[] args)
    {
        var chatId = inputMessage.Chat.Id;
        var commandsAsString =string.Join("\n",  GetCommands().Select(c => $"/{c.Command} - {c.Description}"));
        var help =
            @$"
How to:

* Get SteamID: https://help.steampowered.com/en/faqs/view/2816-BE67-5B69-0FEC
* run /add SteamID
* wait for the next sync (by default it runs every 15 minutes)
* run /play

{commandsAsString}";
        
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

    public static BotCommand[] GetCommands()
    {
        return new[]
        {
            new BotCommand {Command = "list", Description = "returns players ready to play"},
            new BotCommand {Command = "add", Description = "register to play. Arguments: [SteamPlayerId:int]. Example: /add 123"},
            new BotCommand {Command = "play", Description = "provides a list of common multiplayer games, search case-insensitive. Arguments: [category name: string]. Example /play \"Online pvp\" mmo"},
            new BotCommand {Command = "categories", Description = "list of game categories"},
            new BotCommand {Command = "sync", Description = "sync players & games. Arguments: [SteamPlayerId:int]. Example: /sync 123"},
            new BotCommand {Command = "status", Description = "status of the scraper sync"}
        };
    }
}

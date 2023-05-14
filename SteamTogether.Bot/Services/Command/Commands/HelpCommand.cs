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
* run /register SteamID
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
            new BotCommand {Command = "register", Description = "register to play. Arguments: [SteamPlayerId:int]. Example: /register 123"},
            new BotCommand {Command = "cancel", Description = "cancel registration"},
            new BotCommand {Command = "pollstart", Description = "starts the poll \"who wants to play?\""},
            new BotCommand {Command = "pollend", Description = "ends the poll and lists common games to play"},
            new BotCommand {Command = "categories", Description = "list of game categories"},
            new BotCommand {Command = "sync", Description = "sync players & games. Arguments: [SteamPlayerId:int]. Example: /sync 123"},
            new BotCommand {Command = "status", Description = "status of the scraper sync"}
        };
    }
}

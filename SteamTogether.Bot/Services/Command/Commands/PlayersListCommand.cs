using Microsoft.EntityFrameworkCore;
using SteamTogether.Bot.Context;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Command.Commands;

public class PlayersListCommand : ITelegramCommand
{
    public const string Name = "list";
    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PlayersListCommand> _logger;

    public PlayersListCommand(ITelegramBotClient telegramClient, ApplicationDbContext dbContext, ILogger<PlayersListCommand> logger)
    {
        _telegramClient = telegramClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(Message inputMessage, IEnumerable<string> arguments)
    {
        _logger.LogInformation("Executing `{Name}` command with args: {Arguments}", Name, arguments);

        var chat = await _dbContext.TelegramChat
            .Include(chat => chat.Players)
            .FirstOrDefaultAsync(chat => chat.ChatId == 1);

        if (chat != null)
        {
            var list = string.Join(", ", chat.Players.Select(p => p.PlayerId));
            await _telegramClient.SendTextMessageAsync(
                chatId: inputMessage.Chat.Id,
                text: $"Players: {list}",
                cancellationToken: new CancellationToken()
            );
        }
    }
}
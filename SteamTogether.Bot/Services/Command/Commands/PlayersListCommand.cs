using SteamTogether.Core.Context;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Command.Commands;

public class PlayersListCommand : ITelegramCommand
{
    public const string Name = "list";
    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PlayersListCommand> _logger;

    public PlayersListCommand(
        ITelegramBotClient telegramClient,
        ApplicationDbContext dbContext,
        ILogger<PlayersListCommand> logger
    )
    {
        _telegramClient = telegramClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(Message inputMessage, string[] args)
    {
        /*var chat = await _dbContext.TelegramChat
            .Include(chat => chat.Players)
            .FirstOrDefaultAsync(chat => chat.ChatId == inputMessage.Chat.Id);

        if (chat != null)
        {
            var list = string.Join(", ", chat.Players.Select(p => p.PlayerId));
            await _telegramClient.SendTextMessageAsync(
                chatId: inputMessage.Chat.Id,
                text: $"Players: {list}",
                cancellationToken: new CancellationToken()
            );
            return;
        }

        await _telegramClient.SendTextMessageAsync(
            chatId: inputMessage.Chat.Id,
            text: "The list is empty",
            cancellationToken: new CancellationToken()
        );*/
    }
}

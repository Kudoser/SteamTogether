using SteamTogether.Core.Context;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SteamTogether.Bot.Services.Command.Commands;

public class CategoriesCommand : ITelegramCommand
{
    public const string Name = "categories";
    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CategoriesCommand> _logger;

    public CategoriesCommand(
        ITelegramBotClient telegramClient,
        ApplicationDbContext dbContext,
        ILogger<CategoriesCommand> logger
    )
    {
        _telegramClient = telegramClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(Message inputMessage, string[] args)
    {
        var chatId = inputMessage.Chat.Id;

        var categories = _dbContext.SteamGamesCategories.ToArray();
        if (!categories.Any())
        {
            await SendMessage(chatId, "Categories have not been added yet");
            return;
        }
        
        var lines = categories.Select(category => $"{category.Description}({category.CategoryId})");
        await SendMessage(chatId, string.Join("\n", lines));
    }

    private async Task SendMessage(long chatId, string message)
    {
        await _telegramClient.SendTextMessageAsync(
            parseMode: ParseMode.Html,
            chatId: chatId,
            text: message,
            cancellationToken: new CancellationToken()
        );
    }
}

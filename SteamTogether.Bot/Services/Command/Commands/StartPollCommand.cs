using SteamTogether.Core.Context;
using SteamTogether.Core.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SteamTogether.Bot.Services.Command.Commands;

public class StartPollCommand : ITelegramCommand
{
    public const string Name = "pollstart";
    
    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<StartPollCommand> _logger;

    public StartPollCommand(
        ITelegramBotClient telegramClient,
        ApplicationDbContext dbContext,
        ILogger<StartPollCommand> logger
    )
    {
        _telegramClient = telegramClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(Message inputMessage, string[] args)
    {
        var chatId = inputMessage.Chat.Id;

        var poll = _dbContext.TelegramPolls.FirstOrDefault(p => p.ChatId == chatId);
        if (poll != null)
        {
            await SendMessageAsync(chatId, "Only one poll can be active at a time");
            return;
        }
        
        var message = await _telegramClient.SendPollAsync(
            chatId,
            "Who will play?",
            new[] {"Me", "Pass"},
            null,
            false,
            PollType.Regular
        );

        poll = new TelegramPoll
        {
            ChatId = chatId,
            MessageId = message.MessageId,
            PollId = message.Poll?.Id
        };

        _dbContext.TelegramPolls.Add(poll);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SendMessageAsync(long chatId, string message)
    {
        await _telegramClient.SendTextMessageAsync(
            parseMode: ParseMode.Html,
            chatId: chatId,
            text: message,
            cancellationToken: new CancellationToken()
        );
    }
}

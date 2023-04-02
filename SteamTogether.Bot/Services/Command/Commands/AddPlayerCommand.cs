using Microsoft.EntityFrameworkCore;
using SteamTogether.Bot.Context;
using SteamTogether.Bot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Command.Commands;

public class AddPlayerListCommand : ITelegramCommand
{
    public const string Name = "add";
    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AddPlayerListCommand> _logger;

    public AddPlayerListCommand(ITelegramBotClient telegramClient, ApplicationDbContext dbContext,
        ILogger<AddPlayerListCommand> logger)
    {
        _telegramClient = telegramClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(Message inputMessage, IEnumerable<string> args)
    {
        var chatId = inputMessage.Chat.Id;
        var playerIds = args;

        var chat = _dbContext.TelegramChat
            .Where(chat => chat.ChatId == chatId)
            .Include(c => c.Players)
            .FirstOrDefault();
        
        if (chat == null)
        {
            chat = new TelegramChat {ChatId = chatId};
        }
        
        var count = 0;
        foreach (var playerId in playerIds)
        {
            if (!chat.Players.Select(p => p.PlayerId).Contains(playerId))
            {
                // @todo using steam api check that player id exists
                chat.Players.Add(new SteamPlayer {PlayerId = playerId});
                count++;
            }
        }

        await _dbContext.SaveChangesAsync();
        
        await _telegramClient.SendTextMessageAsync(
            chatId: inputMessage.Chat.Id,
            text: $"{count} players were added",
            cancellationToken: new CancellationToken()
        );
        
    }
}
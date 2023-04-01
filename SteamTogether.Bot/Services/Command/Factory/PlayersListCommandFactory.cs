using SteamTogether.Bot.Context;
using SteamTogether.Bot.Services.Command.Commands;
using Telegram.Bot;

namespace SteamTogether.Bot.Services.Command.Factory;

public class PlayersListCommandFactory : ITelegramListCommandFactory
{
    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILoggerFactory _loggerFactory;

    public PlayersListCommandFactory(ITelegramBotClient telegramClient, ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
    {
        _telegramClient = telegramClient;
        _dbContext = dbContext;
        _loggerFactory = loggerFactory;
    }

    public ITelegramCommand Create()
    {
        var logger = _loggerFactory.CreateLogger<PlayersListCommand>();
        return new PlayersListCommand(_telegramClient, _dbContext, logger);
    }
}
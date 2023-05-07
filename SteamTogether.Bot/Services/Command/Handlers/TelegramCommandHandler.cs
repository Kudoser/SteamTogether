using SteamTogether.Bot.Services.Command.Commands;
using SteamTogether.Core.Context;
using SteamTogether.Core.Exceptions;
using SteamTogether.Core.Services.Steam;
using Telegram.Bot;

namespace SteamTogether.Bot.Services.Command.Handlers;

public class TelegramCommandHandler : ITelegramCommandHandler
{
    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ISteamService _steamService;
    private readonly IScraperCommandClient _scraperCommandClient;
    private readonly ILoggerFactory _loggerFactory;

    public TelegramCommandHandler(
        ITelegramBotClient telegramClient,
        ISteamService steamService,
        IScraperCommandClient scraperCommandClient,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory)
    {
        _telegramClient = telegramClient;
        _steamService = steamService;
        _loggerFactory = loggerFactory;
        _scraperCommandClient = scraperCommandClient;

        _dbContext = dbContext;
    }

    public ITelegramCommand Resolve(string name)
    {
        if (name == PlayersListCommand.Name)
        {
            var logger = _loggerFactory.CreateLogger<PlayersListCommand>();
            return new PlayersListCommand(_telegramClient, _dbContext, logger);
        }

        if (name == AddPlayerListCommand.Name)
        {
            var logger = _loggerFactory.CreateLogger<AddPlayerListCommand>();
            return new AddPlayerListCommand(_telegramClient, _dbContext, _steamService, logger);
        }

        if (name == PlayCommand.Name)
        {
            var logger = _loggerFactory.CreateLogger<PlayCommand>();
            return new PlayCommand(_telegramClient, _dbContext, logger);
        }

        if (name == HelpCommand.Name)
        {
            return new HelpCommand(_telegramClient);
        }

        if (name == CategoriesCommand.Name)
        {
            var logger = _loggerFactory.CreateLogger<CategoriesCommand>();
            return new CategoriesCommand(_telegramClient, _dbContext, logger);
        }

        if (name == SyncCommand.Name)
        {
            return new SyncCommand(_telegramClient, _scraperCommandClient);
        }

        throw new UnknownCommandException($"Unknown command name={name}]");
    }
}

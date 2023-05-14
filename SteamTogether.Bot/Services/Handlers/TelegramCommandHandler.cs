using SteamTogether.Bot.Services.Command.Commands;
using SteamTogether.Core.Context;
using SteamTogether.Core.Exceptions;
using SteamTogether.Core.Services.Steam;
using Telegram.Bot;

namespace SteamTogether.Bot.Services.Handlers;

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
        if (name == RegisterCommand.Name)
        {
            var logger = _loggerFactory.CreateLogger<RegisterCommand>();
            return new RegisterCommand(_telegramClient, _dbContext, _steamService, logger);
        }

        if (name == CancelRegisterCommand.Name)
        {
            return new CancelRegisterCommand(_telegramClient, _dbContext);
        }

        if (name == StartPollCommand.Name)
        {
            var logger = _loggerFactory.CreateLogger<StartPollCommand>();
            return new StartPollCommand(_telegramClient, _dbContext, logger);
        }

        if (name == EndPollCommand.Name)
        {
            var logger = _loggerFactory.CreateLogger<EndPollCommand>();
            return new EndPollCommand(_telegramClient, _dbContext);
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
            var logger = _loggerFactory.CreateLogger<SyncCommand>();
            return new SyncCommand(_telegramClient, _scraperCommandClient, logger);
        }

        if (name == ScraperStatusCommand.Name)
        {
            return new ScraperStatusCommand(_telegramClient, _scraperCommandClient);
        }

        throw new UnknownCommandException($"Unknown command name={name}]");
    }
}

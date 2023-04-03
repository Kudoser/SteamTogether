using SteamTogether.Bot.Context;
using SteamTogether.Bot.Services.Command.Commands;
using SteamTogether.Core.Exceptions;
using SteamTogether.Core.Services.Steam;
using Telegram.Bot;

namespace SteamTogether.Bot.Services.Command.Handlers;

public class TelegramCommandHandler : ITelegramCommandHandler
{
    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ISteamService _steamService;
    private readonly ILoggerFactory _loggerFactory;

    public TelegramCommandHandler(
        ITelegramBotClient telegramClient,
        ApplicationDbContext dbContext,
        ISteamService steamService,
        ILoggerFactory loggerFactory
    )
    {
        _telegramClient = telegramClient;
        _dbContext = dbContext;
        _steamService = steamService;
        _loggerFactory = loggerFactory;
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

        throw new UnknownCommandException($"Unknown command name={name}]");
    }
}

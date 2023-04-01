using SteamTogether.Bot.Exceptions;
using SteamTogether.Bot.Services.Command.Commands;
using SteamTogether.Bot.Services.Command.Factory;

namespace SteamTogether.Bot.Services.Command.Handlers;

public class TelegramCommandHandler : ITelegramCommandHandler
{
    private readonly ITelegramListCommandFactory _listCommandFactory;

    public TelegramCommandHandler(ITelegramListCommandFactory listCommandFactory)
    {
        _listCommandFactory = listCommandFactory;
    }

    public ITelegramCommand Resolve(string name)
    {
        if (name == PlayersListCommand.Name)
        {
            return _listCommandFactory.Create();
        }

        throw new UnknownCommandException($"Unknown command name={name}]");
    }
}
using SteamTogether.Bot.Services.Command.Commands;

namespace SteamTogether.Bot.Services.Command.Handlers;

public interface ITelegramCommandHandler
{
    ITelegramCommand Resolve(string name);
}

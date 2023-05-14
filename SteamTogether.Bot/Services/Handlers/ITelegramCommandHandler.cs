using SteamTogether.Bot.Services.Command.Commands;

namespace SteamTogether.Bot.Services.Handlers;

public interface ITelegramCommandHandler
{
    ITelegramCommand Resolve(string name);
}

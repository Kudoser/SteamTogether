using SteamTogether.Bot.Services.Command.Commands;

namespace SteamTogether.Bot.Services.Command.Factory;

public interface ITelegramCommandFactory
{
    public ITelegramCommand Create();
}
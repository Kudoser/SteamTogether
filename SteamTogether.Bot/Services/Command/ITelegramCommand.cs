using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Command;

public interface ITelegramCommand
{
    public Task ExecuteAsync(Message inputMessage, IEnumerable<string> arguments);
}
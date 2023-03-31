namespace SteamTogether.Bot.Services;

public interface ITelegramService
{
    public Task StartReceivingAsync(CancellationToken cancellationToken);
}
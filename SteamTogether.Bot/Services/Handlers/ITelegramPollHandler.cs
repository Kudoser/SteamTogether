using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Handlers;

public interface ITelegramPollHandler
{
    public Task HandlePollAnswer(PollAnswer pollAnswer, CancellationToken ct);
}
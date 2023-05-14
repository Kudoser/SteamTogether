using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Handlers;

public interface ITelegramPollHandler
{
    public Task HandlePollAsync(Poll poll, CancellationToken ct);
    public Task HandlePollAnswer(PollAnswer pollAnswer, CancellationToken ct);
}
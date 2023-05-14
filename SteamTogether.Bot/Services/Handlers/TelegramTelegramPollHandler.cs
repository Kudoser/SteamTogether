using Microsoft.EntityFrameworkCore;
using SteamTogether.Core.Context;
using SteamTogether.Core.Models;
using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Handlers;

public class TelegramTelegramPollHandler : ITelegramPollHandler
{
    private readonly ApplicationDbContext _dbContext;

    public TelegramTelegramPollHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandlePollAnswer(PollAnswer pollAnswer, CancellationToken ct)
    {
        var poll = _dbContext.TelegramPolls
            .Where(p => p.PollId == pollAnswer.PollId)
            .Include(p => p.TelegramPollVotes)
            .FirstOrDefault();

        var pollVote = poll?.TelegramPollVotes
            .FirstOrDefault(p => p.TelegramUserId == pollAnswer.User.Id);
        
        if (!pollAnswer.OptionIds.Any())
        {
            if (pollVote != null)
            {
                _dbContext.TelegramPollVotes.Remove(pollVote);
                await _dbContext.SaveChangesAsync(ct);
            }
        }
        else
        {
            var vote = pollAnswer.OptionIds.FirstOrDefault();
            if (vote == 0 && pollVote == null)
            {
                pollVote = new TelegramPollVote
                {
                    PollId = pollAnswer.PollId,
                    TelegramUserId = pollAnswer.User.Id
                };
                _dbContext.TelegramPollVotes.Add(pollVote);
                await _dbContext.SaveChangesAsync(ct);
            }
        }
    }
}
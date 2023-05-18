using Microsoft.EntityFrameworkCore;
using SteamTogether.Bot.Models;
using SteamTogether.Core.Context;
using SteamTogether.Core.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SteamTogether.Bot.Services.Command.Commands;

public class EndPollCommand : ITelegramCommand
{
    public const string Name = "pollend";

    private readonly ITelegramBotClient _telegramClient;
    private readonly ApplicationDbContext _dbContext;

    private const string DefaultGameCategory = "Co-op";

    public EndPollCommand(
        ITelegramBotClient telegramClient,
        ApplicationDbContext dbContext
    )
    {
        _telegramClient = telegramClient;
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(Message inputMessage, string[] args)
    {
        var chatId = inputMessage.Chat.Id;

        var telegramPoll = _dbContext.TelegramPolls
            .Where(p => p.ChatId == chatId)
            .Include(p => p.TelegramPollVotes)
            .FirstOrDefault();

        if (telegramPoll == null)
        {
            await SendMessageAsync(chatId, "No polls started");
            return;
        }

        var poll = await _telegramClient.StopPollAsync(
            chatId,
            telegramPoll.MessageId
        );
        
        _dbContext.TelegramPolls.Remove(telegramPoll);
        await _dbContext.SaveChangesAsync();

        if (poll.TotalVoterCount <= 0)
        {
            await SendMessageAsync(chatId, "Nobody voted");
            return;
        }

        var categoryNames = args.Any()
            ? args
            : new[] {DefaultGameCategory};

        var categories = _dbContext.SteamGamesCategories
            .Where(c => categoryNames.Select(_ => _.ToLower()).Contains(c.Description.ToLower()))
            .ToArray();

        var categoryIds = categories
            .Select(c => c.CategoryId)
            .ToArray();

        if (!categoryIds.Any())
        {
            await SendMessageAsync(chatId, "Can't find such categories");
            return;
        }

        var userIds = telegramPoll.TelegramPollVotes
            .Where(pv => pv.PollId == poll.Id)
            .Select(p => p.TelegramUserId)
            .ToArray();

        if (!userIds.Any())
        {
            await SendMessageAsync(chatId, "No users found");
            return;
        }

        var games = new GameCollector(_dbContext).GetGroupGames(userIds, categoryIds, 15);
        if (!games.Any())
        {
            await SendMessageAsync(chatId, "No games found");
            return;
        }

        var lines = RenderLines(games, categories);
        await SendMessageAsync(chatId, string.Join("\n", lines));
    }

    private List<string> RenderLines(GroupedGameResult[] games, SteamGameCategory[] categories)
    {
        var categoryLines = categories
            .Select(c => $"{c.Description}")
            .ToArray();

        var messageLines = new List<string> {$"Categories: {string.Join(",", categoryLines)}"};
        var lines = games.Select(
            (g, i) =>
            {
                var avgTime = TimeSpan.FromSeconds(g.TotalSeconds.Average()).ToString(@"dd\d\ hh\h\ mm\s");
                return $"{i + 1}. {g.Name}, count: {g.Count}, avg time: {avgTime} ({string.Join(",", g.PlayerNames)})";
            }).ToArray();

        messageLines.AddRange(lines);
        return messageLines;
    }

    private async Task SendMessageAsync(long chatId, string message)
    {
        await _telegramClient.SendTextMessageAsync(
            parseMode: ParseMode.Html,
            chatId: chatId,
            text: message,
            cancellationToken: new CancellationToken()
        );
    }
}
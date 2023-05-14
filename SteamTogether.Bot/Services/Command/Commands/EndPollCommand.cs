using Microsoft.EntityFrameworkCore;
using SteamTogether.Core.Context;
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
            .Select(p => p.TelegramUserId)
            .ToArray();

        var games = _dbContext.TelegramChatParticipants
            .Where(p => userIds.Contains(p.TelegramUserId))
            .Include(p => p.SteamPlayer)
            .ThenInclude(sp => sp.Games)
            .SelectMany(
                participant =>
                    participant.SteamPlayer.Games.Where(game =>
                        game.Categories.Any(c => categoryIds.Contains(c.CategoryId))),
                (participant, game) =>
                    new
                    {
                        PlayerName = participant.SteamPlayer.Name,
                        GameName = game.Name,
                        game.GameId
                    }
            )
            .GroupBy(p => new {p.GameId, p.GameName})
            .Select(
                g =>
                    new
                    {
                        Name = g.Key.GameName,
                        Count = g.Count(),
                        Players = string.Join(",", g.Select(p => p.PlayerName))
                    }
            )
            .OrderByDescending(x => x.Count)
            .Take(15)
            .ToArray();

        if (!games.Any())
        {
            await SendMessageAsync(chatId, "No games found");
            return;
        }

        var categoryLines = categories
            .Select(c => $"{c.Description}")
            .ToArray();

        var messageLines = new List<string> {$"Categories: {string.Join(",", categoryLines)}"};

        var lines = games.Select(
            (g, i) => $"{i + 1}. {g.Name}, count: {g.Count} ({g.Players})"
        ).ToArray();

        messageLines.AddRange(lines);
        await SendMessageAsync(chatId, string.Join("\n", messageLines));

        _dbContext.TelegramPolls.Remove(telegramPoll);
        await _dbContext.SaveChangesAsync();
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
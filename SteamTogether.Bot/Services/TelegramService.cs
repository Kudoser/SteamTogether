using SteamTogether.Bot.Services.Command.Commands;
using SteamTogether.Bot.Services.Command.Parser;
using SteamTogether.Bot.Services.Handlers;
using SteamTogether.Core.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SteamTogether.Bot.Services;

public class TelegramService : ITelegramService
{
    private readonly ITelegramBotClient _client;
    private readonly ITelegramCommandParser _telegramCommandParser;
    private readonly ITelegramCommandHandler _telegramCommandHandler;
    private readonly ITelegramPollHandler _telegramPollHandler;
    private readonly ILogger<ITelegramService> _logger;

    private string? BotName { get; set; }

    public TelegramService(
        ITelegramBotClient client,
        ITelegramCommandParser telegramCommandParser,
        ITelegramCommandHandler telegramCommandHandler,
        ITelegramPollHandler telegramPollHandler,
        ILogger<TelegramService> logger
    )
    {
        _client = client;
        _telegramCommandParser = telegramCommandParser;
        _telegramCommandHandler = telegramCommandHandler;
        _telegramPollHandler = telegramPollHandler;
        _logger = logger;
    }

    public async Task StartReceivingAsync(CancellationToken cancellationToken)
    {
        var me = await _client.GetMeAsync(cancellationToken);
        _logger.LogInformation("Connected to {BotName}, starting receiving messages...", me.Username);
        BotName = me.Username;

        _logger.LogInformation("Updating commands list");
        await _client.SetMyCommandsAsync(HelpCommand.GetCommands(), cancellationToken: cancellationToken);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] {UpdateType.Poll, UpdateType.Message, UpdateType.PollAnswer}
        };
        await _client.ReceiveAsync(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken ct
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(BotName);
        
        if (update.Message is { } message)
        {
            await HandleCommand(botClient, message, ct);
            return;
        }
        
        if (update.Poll is { } poll)
        {
            await _telegramPollHandler.HandlePollAsync(poll, ct);
            return;
        }

        if (update.PollAnswer is { } pollAnswer)
        {
            await _telegramPollHandler.HandlePollAnswer(pollAnswer, ct);
            return;
        }
    }
    private async Task HandleCommand(ITelegramBotClient botClient, Message message, CancellationToken ct)
    {
        var chatId = message.Chat.Id;

        var botCommand = message.Entities?.FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);
        if (botCommand == null)
        {
            return;
        }

        try
        {
            var parsedResult = _telegramCommandParser.Parse(message.Text, BotName);
            if (!parsedResult.Parsed)
            {
                return;
            }

            var command = _telegramCommandHandler.Resolve(parsedResult.CommandName);

            await command.ExecuteAsync(message, parsedResult.Arguments);
        }
        catch (UnknownCommandException e)
        {
            _logger.LogWarning("An error occured during command execution: {Error}", e.Message);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Can't recognize command",
                cancellationToken: ct
            );
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred during command execution: {Error}", e.Message);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"An error occurred during command execution: {e.Message}",
                cancellationToken: ct
            );
        }
    }

    private Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError("polling error: {Error}", errorMessage);
        return Task.CompletedTask;
    }
}
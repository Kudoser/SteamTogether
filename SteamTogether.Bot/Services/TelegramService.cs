using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SteamTogether.Bot.Services;

public class TelegramService : ITelegramService
{
    private readonly ITelegramBotClient _client;
    private readonly ILogger<ITelegramService> _logger;

    public TelegramService(ITelegramBotClient client, ILogger<TelegramService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task StartReceivingAsync(CancellationToken cancellationToken)
    {
        using CancellationTokenSource cts = new();

        ReceiverOptions receiverOptions =
            new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

        var me = await _client.GetMeAsync(cancellationToken);
        _logger.LogInformation(
            "Connected to {BotName}, starting receiving messages...",
            me.Username
        );

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
        CancellationToken cancellationToken
    )
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;
        _logger.LogInformation(
            "Received a '{MessageText}' message in chat {ChatId}",
            message.Text,
            message.Chat.Id
        );

        // Echo received message text
        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "You said:\n" + messageText,
            cancellationToken: cancellationToken
        );
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

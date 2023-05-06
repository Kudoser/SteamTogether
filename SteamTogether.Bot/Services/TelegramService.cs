using SteamTogether.Bot.Services.Command.Commands;
using SteamTogether.Bot.Services.Command.Handlers;
using SteamTogether.Bot.Services.Command.Parser;
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
    private readonly ILogger<ITelegramService> _logger;

    public TelegramService(
        ITelegramBotClient client,
        ITelegramCommandParser telegramCommandParser,
        ITelegramCommandHandler telegramCommandHandler,
        ILogger<TelegramService> logger
    )
    {
        _client = client;
        _telegramCommandParser = telegramCommandParser;
        _telegramCommandHandler = telegramCommandHandler;
        _logger = logger;
    }

    public async Task StartReceivingAsync(CancellationToken cancellationToken)
    {
        ReceiverOptions receiverOptions = new() { AllowedUpdates = Array.Empty<UpdateType>() };

        var me = await _client.GetMeAsync(cancellationToken);
        _logger.LogInformation(
            "Connected to {BotName}, starting receiving messages...",
            me.Username
        );

        await _client.SetMyCommandsAsync(HelpCommand.GetCommands(), cancellationToken:cancellationToken);

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
        
        var botCommand = message.Entities?.FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);
        if (botCommand == null)
        {
            return;
        }
        
        try
        {
            var parsedResult = _telegramCommandParser.Parse(message.Text);
            if (!parsedResult.Parsed)
            {
                return;
            }

            var command = _telegramCommandHandler.Resolve(parsedResult.CommandName);

            await command.ExecuteAsync(update.Message, parsedResult.Arguments);
        }
        catch (UnknownCommandException e)
        {
            _logger.LogWarning("An error occured during command execution: {Error}", e.Message);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Can't recognize command",
                cancellationToken: cancellationToken
            );
        }
        catch (Exception e)
        {
            _logger.LogError("An error occured during command execution: {Error}", e.Message);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Unexpected error",
                cancellationToken: cancellationToken
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

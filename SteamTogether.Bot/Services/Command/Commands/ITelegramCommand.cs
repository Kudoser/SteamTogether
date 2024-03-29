﻿using Telegram.Bot.Types;

namespace SteamTogether.Bot.Services.Command.Commands;

public interface ITelegramCommand
{
    public Task ExecuteAsync(Message inputMessage, string[] args);
}

# SteamTogether Telegram Bot

A simple telegram bot based on [C# Telegram bot](https://github.com/TelegramBots/Telegram.Bot) to find games to play with friends.

## How it works

@todo

## Getting Started

### How to run

- Get a token for your bot <https://core.telegram.org/bots#how-do-i-create-a-bot>
- Run the app, providing the token:

  - as a command line argument:

    ```shell
    > .\SteamTogether.Bot.exe
        --Telegram:Token="YOUR_TOKEN"
        --Steam:ApiKey="YOUR_STEAM_API_KEY"
        --Database:ConnectionString="Data Source=together.db"
    ```

  - or as an environment variable:

    ```shell
    > set BOT_Telegram__Token="YOUR_TELEGRAM_TOKEN"
    > set BOT_Steam__ApiKey="YOUR_STEAM_API_KEY"
    > set BOT_Database__ConnectionString="Data Source=together.db"
    > set BOT_HealthCheck__Port="10090"
    > .\SteamTogether.Bot.exe
    ```

## Telegram bot

### How to define commands for your bot

- go to <https://t.me/BotFather>
- use command `/setcommands`
- choose the bot
- post the list of supported commands from down below

### Supported commands

```text
help - how to
list - returns the list of Steam players at the same chat ready to participate
add - adds a new Steam player id (@todo: and optionally Steam API key). Example: /add 123
play - provides a list of common multiplayer games, params: category name. Case-insensitive. Example: /play mmo "Online Pvp". Default category: co-op
categories - provides a list of all possible games categories
```

## Health check

The server exposes a health check HTTP endpoint on `HealthCheck:Port`.
Listen to all HTTP requests and replies with HTTP 200

# SteamTogether Telegram Bot

A simple telegram bot based on [C# Telegram bot](https://github.com/TelegramBots/Telegram.Bot) to find games to play with friends.

## How it works

@todo

## Getting Started

### How to run

* Get a token for your bot <https://core.telegram.org/bots#how-do-i-create-a-bot>
* Run the app, providing the token:
  * as a command line argument:

    ```shell
    > .\SteamTogether.Bot.exe  --Telegram:Token="YOUR_TOKEN" --Database:ConnectionString="Data Source=together.db"
    ```

  * or as an environment variable:

    ```shell
    > set BOT_Telegram__Token="YOUR_TELEGRAM_TOKEN"
    > set BOT_Database__ConnectionString="Data Source=together.db"
    > .\SteamTogether.Bot.exe
    ``` 
    
## Telegram bot
### Supported commands
* `list - returns the list of steam players at the same chat ready to participate`
* `add - adds a new steam player id (@todo: and optionally steam api key)`
* `play - provides a list of common multiplayer games`

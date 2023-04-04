# SteamTogether

Telegram bot which helps to organize online game sessions with buddies.

## Usage

Add @SteamTogetherBot to your group chat and type `/help` to get list of available commands.

TODO: add here the output of `/help` command.

## Development

There are the following projects:

- **[SteamTogether.Bot](./SteamTogether.Bot)** with tests in [SteamTogether.Bot.UnitTests](./SteamTogether.Bot.UnitTests)
  Telegram bot which is responsible for handling user commands and sending notifications.
- **[SteamTogether.Scraper](./SteamTogether.Scraper)** with tests in [SteamTogether.Scraper.UnitTests](./SteamTogether.Scraper.UnitTests)
  Background service which is responsible for scraping Steam for game metadata and writing it into the database.

See the corresponding readme files for more info.

## Database 

### Migrations

Go to SteamTogether.Core project
```shell
> cd SteamTogether.Core
```

make changes to models and create migration
```shell
> dotnet ef migrations Add MyMigrationName --project SteamTogether.Core.csproj --startup-project ../SteamTogether.Bot/SteamTogether.Bot.csproj
```

Apply changes to a database
```shell
> dotnet ef database update Init --project SteamTogether.Core.csproj --startup-project ../SteamTogether.Bot/SteamTogether.Bot.csproj
```

[EF migrations documentation link](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)
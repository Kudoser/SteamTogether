# SteamTogether Scrapper

Scrape games from steam

## How it works

@todo

## Getting Started

### How to run

* Get a steam api key here https://steamcommunity.com/dev/apikey
* Run the app, providing the api key:
  * as a command line argument:

    ```shell
    > .\SteamTogether.Bot.exe  --Scraper:Schedule="* */5 * * *" --Steam:ApiKey="YOUR_API_KEY" --Database:ConnectionString="Data Source=together.db"
    ```

  * or as an environment variable:

    ```shell
    > set SCRAPER_Steam__ApiKey="YOUR_API_KEY"
    > set SCRAPER_Scraper__Schedule="* */5 * * *"
    > set BOT_Database__ConnectionString="Data Source=together.db"
    > .\SteamTogether.Bot.exe
    ```
### Arguments:

*  `Schedule` — uses crontab format, for example `"* */5 * * *"` will run scrapper every 5 hours
https://github.com/atifaziz/NCrontab#ncrontab-crontab-for-net
* `RunOnStartup` — true/false, if true — triggers code right after host is started and then according to the schedule
* `PlayersSyncPeriodSeconds` - period in seconds to sync players, default = 18000 (5 hours)
* `GamesSyncPeriodMinutes` - period in minutes to sync games, default = 10080 (7 days)
* `PlayersPerRun` - how many players to process per one run
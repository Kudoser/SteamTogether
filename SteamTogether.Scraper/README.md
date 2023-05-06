# SteamTogether Scrapper

Scrape games from steam

## How it works

@todo

## Getting Started

### How to run

- Get a Steam API key here: <https://steamcommunity.com/dev/apikey>
- Run the app, providing the API key:

  - as a command line argument:

    ```shell
    > .\SteamTogether.Bot.exe
          --Scraper:Schedule="0 0 */5 * * *"
          --Steam:ApiKey="YOUR_API_KEY"
          --Database:ConnectionString="Data Source=together.db"
    ```

  - or as an environment variable:

    ```shell
    > set SCRAPER_Steam__ApiKey="YOUR_API_KEY"
    > set SCRAPER_Scraper__Schedule="0 0 */5 * * *"
    > set SCRAPER_Database__ConnectionString="Data Source=together.db"
    > .\SteamTogether.Bot.exe
    ```

### Arguments

- `Schedule` (required) - sets a schedule on which to run scraping.
  Uses ncrontab format with seconds, for example, `0 0 */5 * * *` will run scrapper every 5 hours.
  See [implementation](https://github.com/atifaziz/NCrontab#ncrontab-crontab-for-net) for more details.
- `RunOnStartup` - if set to `true`, runs scraping right after the host is started, in addition to the schedule, default = false.
- `PlayersSyncPeriodSeconds` - a period in seconds to sync players, default = 18000 (5 hours).
- `GamesSyncPeriodMinutes` - a period in minutes to sync games, default = 10080 (7 days).
- `PlayersPerRun` - how many players to process per one run, default = 10.
- `HttpCommandPort` - http listener port, default = 10091.

## Http listener commands

### Status

* Command - enum [Status = 0, Sync = 1]

#### Request: 
```http request
GET http://localhost:8080 {"Command": 0}
```

#### Response: 200 OK 
```json
{
    "Status": 0
}
```

### Sync

#### Request:
```http request
GET http://localhost:8080 {"Command": 1, "Arguments": ["123456789"]}
```
* Result - enum [Success|Busy]

#### Response: 200 OK
```json
{
    "Result": "Success"
}
```

#### Response: 503 ServiceUnavailable
```json
{
    "Result": "Busy"
}
```
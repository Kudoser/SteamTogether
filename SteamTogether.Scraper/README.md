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
    > .\SteamTogether.Bot.exe  --Steam:ApiKey="YOUR_API_KEY" --Database:ConnectionString="Data Source=together.db"
    ```

  * or as an environment variable:

    ```shell
    > set SCRAPER_Steam__ApiKey="YOUR_API_KEY"
    > set BOT_Database__ConnectionString="Data Source=together.db"
    > .\SteamTogether.Bot.exe
    ```

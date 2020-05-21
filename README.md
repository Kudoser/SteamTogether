# SteamTogether

A simple project which may help to find intersections in games with your friends

## How it works

Takes players by given steam ids and download information about their games.
Calculating intersections and return result like that:
```
GauntletT, count: 3 (monkey,donkey,dog)
Alien Swarm: Reactive Drop, count: 2 (monkey,dog)
Half-Life: Opposing Force, count: 2 (monkey,donkey)
```

## Getting Started

How to run

* Take a copy of appsettings.example.json
* Rename it to appsettings.json and fill params with actual values (information about params you might find down below)
* Make appsettings.json file to be copied once project build, right click on it -> properties, editable -> copy to output directory -> copy if newer
* run

## Options for appsettins.json
* SteamDevKey ``take your key here https://steamcommunity.com/dev/apikey``
* FilterCount `` define numbers of intersections below this value``
* Users ``the array of user steam ids with whom you want to find intersections``
* fullEqual ``outputs only those games which owned by every player, if true then ignores FilterCount``


## Dependencies

* SteamWebAPI2
* Microsoft.Extensions.Configuration
* Microsoft.Extensions.Configuration.Json
* Microsoft.Extensions.Configuration.Binder

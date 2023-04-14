#!/usr/bin/env bash

set -euo pipefail

dotnet ef database update \
    --project SteamTogether.Core/SteamTogether.Core.csproj \
    --startup-project SteamTogether.Bot/SteamTogether.Bot.csproj

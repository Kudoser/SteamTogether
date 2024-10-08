# syntax=docker/dockerfile:1.4

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS sdk


FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
# Install curl for the health check
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*


FROM sdk AS restore

WORKDIR /src

# Copy csproj and restore as distinct layers
COPY --link SteamTogether.sln .
COPY --link SteamTogether.Bot/*.csproj SteamTogether.Bot/
COPY --link SteamTogether.Bot.UnitTests/*.csproj SteamTogether.Bot.UnitTests/
COPY --link SteamTogether.Core/*.csproj SteamTogether.Core/
COPY --link SteamTogether.Scraper/*.csproj SteamTogether.Scraper/
COPY --link SteamTogether.Scraper.UnitTests/*.csproj SteamTogether.Scraper.UnitTests/

RUN dotnet restore --no-cache

# Copy everything else
COPY --link SteamTogether.Bot SteamTogether.Bot
COPY --link SteamTogether.Bot.UnitTests SteamTogether.Bot.UnitTests
COPY --link SteamTogether.Core SteamTogether.Core
COPY --link SteamTogether.Scraper SteamTogether.Scraper
COPY --link SteamTogether.Scraper.UnitTests SteamTogether.Scraper.UnitTests


FROM restore AS build-bot
ARG VERSION=¯\_(ツ)_/¯
RUN dotnet publish SteamTogether.Bot/SteamTogether.Bot.csproj \
    --configuration Release \
    --output /publish \
    --no-restore \
    -p:Version=$VERSION


FROM runtime AS bot
WORKDIR /app
COPY --link --from=build-bot /publish .
ENTRYPOINT ["/app/SteamTogether.Bot"]


FROM restore AS build-scraper
ARG VERSION=¯\_(ツ)_/¯
RUN dotnet publish SteamTogether.Scraper/SteamTogether.Scraper.csproj \
    --configuration Release \
    --output /publish \
    --no-restore \
    -p:Version=$VERSION


FROM runtime as scraper
WORKDIR /app
COPY --link --from=build-scraper /publish .
COPY --link docker/scraper/entrypoint.sh .
ENTRYPOINT ["/app/entrypoint.sh"]

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS sdk
FROM mcr.microsoft.com/dotnet/runtime:7.0 AS runtime


FROM sdk AS restore

WORKDIR /src

# Copy csproj and restore as distinct layers
COPY SteamTogether.sln .
COPY SteamTogether.Bot/*.csproj SteamTogether.Bot/
COPY SteamTogether.Bot.UnitTests/*.csproj SteamTogether.Bot.UnitTests/
COPY SteamTogether.Core/*.csproj SteamTogether.Core/
COPY SteamTogether.Scraper/*.csproj SteamTogether.Scraper/

RUN dotnet restore --no-cache

# Copy everything else
COPY . .


FROM restore AS build-bot
RUN dotnet publish SteamTogether.Bot/SteamTogether.Bot.csproj \
    --configuration Release \
    --output /publish \
    --no-restore


FROM runtime AS bot
WORKDIR /app
COPY --from=build-bot /publish .
ENTRYPOINT ["/app/SteamTogether.Bot"]


FROM restore AS build-scraper
RUN dotnet publish SteamTogether.Scraper/SteamTogether.Scraper.csproj \
    --configuration Release \
    --output /publish \
    --no-restore


FROM runtime as scraper
WORKDIR /app
COPY --from=build-scraper /publish .
ENTRYPOINT ["/app/SteamTogether.Scraper"]

FROM restore AS migrator
RUN dotnet tool install --global dotnet-ef --no-cache
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT [ "/src/docker/migrator.sh" ]

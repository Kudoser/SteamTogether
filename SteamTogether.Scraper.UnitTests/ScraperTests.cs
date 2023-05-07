using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Steam.Models.SteamCommunity;
using Steam.Models.SteamStore;
using SteamTogether.Core.Context;
using SteamTogether.Core.Models;
using SteamTogether.Core.Options;
using SteamTogether.Core.Services;
using SteamTogether.Core.Services.Steam;
using SteamTogether.Scraper.Options;
using SteamTogether.Scraper.Services;
using SteamWebAPI2.Utilities;

namespace SteamTogether.Scraper.UnitTests;

public class ScraperTests
{
    private readonly HostBuilder _builder;

    public ScraperTests()
    {
        _builder = new HostBuilder();
        _builder
            .ConfigureAppConfiguration((_, config) =>
            {
                config
                    .AddEnvironmentVariables()
                    .SetBasePath(_.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", true, true);
            })
            .ConfigureServices((builder, services) =>
            {
                services
                    .AddOptions<ScraperOptions>()
                    .Bind(builder.Configuration.GetSection(ScraperOptions.Scraper));

                services
                    .AddOptions<DatabaseOptions>()
                    .Bind(builder.Configuration.GetSection(DatabaseOptions.Database));

                services
                    .AddScoped<IDateTimeService, DateTimeService>()
                    .AddScoped<IScraperService, ScraperService>();

                services
                    .AddLogging(x => x.AddConsole())
                    .AddDbContext<ApplicationDbContext>(
                        opt => { opt.UseInMemoryDatabase("test.db"); });
            });
    }

    [Fact]
    public async Task Should_Sync_NewlyAdded_PlayerTest()
    {
        var mockedSteamService = new Mock<ISteamService>();
        mockedSteamService
            .Setup(x => x.GetOwnedGamesAsync(1))
            .ReturnsAsync(new SteamWebResponse<OwnedGamesResultModel>
            {
                Data = new OwnedGamesResultModel
                {
                    GameCount = 3,
                    OwnedGames = new[]
                    {
                        new OwnedGameModel
                        {
                            AppId = 1,
                            Name = "Second game name"
                        },
                        new OwnedGameModel
                        {
                            AppId = 2
                        },
                        new OwnedGameModel
                        {
                            AppId = 3,
                            Name = "Third game name"
                        },
                    }
                }
            });

        mockedSteamService
            .Setup(x => x.GetAppDetailsAsync(1))
            .ReturnsAsync(new StoreAppDetailsDataModel
            {
                Name = "First game name",
                Categories = new StoreCategoryModel[] {new() {Id = 1}},
                SteamAppId = 1
            });

        mockedSteamService
            .Setup(x => x.GetAppDetailsAsync(2))
            .ReturnsAsync(new StoreAppDetailsDataModel
            {
                Name = "Second game name",
                Categories = new StoreCategoryModel[] {new() {Id = 1}},
                SteamAppId = 2
            });

        mockedSteamService
            .Setup(x => x.GetAppDetailsAsync(3))
            .ReturnsAsync(new StoreAppDetailsDataModel
            {
                Name = "Third game name",
                Categories = new StoreCategoryModel[] {new() {Id = 1}},
                SteamAppId = 3
            });

        _builder.ConfigureServices(services =>
        {
            services.AddSingleton<ISteamService>(_ => mockedSteamService.Object);
            services.AddSingleton<IHostedService, ScraperWorker>();
        });

        var host = _builder.Build();
        var dbContext = host.Services.GetRequiredService<ApplicationDbContext>();
        
        // fill up the database 
        var newPLayer = new SteamPlayer {PlayerId = 1, Name = "John Doe"};
        await dbContext.SteamPlayers.AddAsync(newPLayer);
        await dbContext.SaveChangesAsync();

        var scraper = host.Services.GetRequiredService<IScraperService>();
        await scraper.RunSync();

        var player = dbContext.SteamPlayers
            .Include(player => player.Games)
            .FirstOrDefault(player => player.PlayerId == 1);
        
        Assert.NotNull(player);
        Assert.Equal(3, player.Games.Count);
        Assert.Equal("First game name", player.Games.FirstOrDefault(game => game.SteamAppId == 1)?.Name);
        Assert.Equal("Second game name", player.Games.FirstOrDefault(game => game.SteamAppId == 2)?.Name);
        Assert.Equal("Third game name", player.Games.FirstOrDefault(game => game.SteamAppId == 3)?.Name);
    }
}
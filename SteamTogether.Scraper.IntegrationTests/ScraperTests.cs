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

namespace SteamTogether.Scraper.IntegrationTests;

public class ScraperTests
{
    private readonly IServiceCollection _serviceProvider = new ServiceCollection();

    public ScraperTests()
    {
        var configBuilder = new ConfigurationBuilder();
        var config = configBuilder
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        _serviceProvider
            .AddOptions<ScraperOptions>().Bind(config.GetSection(ScraperOptions.Scraper));

        _serviceProvider
            .AddOptions<DatabaseOptions>()
            .Bind(config.GetSection(DatabaseOptions.Database));

        _serviceProvider
            .AddScoped<IDateTimeService, DateTimeService>()
            .AddScoped<IScrapperService, ScrapperService>()
            .AddSingleton<IHostedService, Worker>();

        _serviceProvider
            .AddLogging(x => x.AddConsole())
            .AddDbContext<ApplicationDbContext>(
                opt =>
                {
                    var connectionString =
                        config.GetSection(DatabaseOptions.Database).GetValue<string>("ConnectionString");
                    ArgumentException.ThrowIfNullOrEmpty(connectionString);

                    opt.UseInMemoryDatabase(connectionString);
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
                SteamAppId = 1
            });

        mockedSteamService
            .Setup(x => x.GetAppDetailsAsync(3))
            .ReturnsAsync(new StoreAppDetailsDataModel
            {
                Name = "Third game name",
                Categories = new StoreCategoryModel[] {new() {Id = 1}},
                SteamAppId = 1
            });


        _serviceProvider.AddSingleton<ISteamService>(_ => mockedSteamService.Object);
        var services = _serviceProvider.BuildServiceProvider();
        var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<Worker>();

        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Running pending migrations");
            await dbContext.Database.MigrateAsync();
        }

        var worker = new Worker(services, logger);

        await dbContext.SteamPlayers.AddAsync(
            new SteamPlayer
            {
                PlayerId = 1,
                Name = "John Doe"
            });
        // @todo
        /*await dbContext.SaveChangesAsync();
        await worker.StartAsync(new CancellationToken());

        var player = dbContext.SteamPlayers.FirstOrDefault(player => player.PlayerId == 1);
        Assert.NotNull(player);
        Assert.Equal(3, player.Games.Count);
        Assert.Equal("First game", player.Games.FirstOrDefault(game => game.SteamAppId == 1)?.Name);
        Assert.Equal("First game", player.Games.FirstOrDefault(game => game.SteamAppId == 2)?.Name);
        Assert.Equal("First game", player.Games.FirstOrDefault(game => game.SteamAppId == 3)?.Name);*/
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteamTogether.Bot.Services.Command.Commands;
using SteamTogether.Core.Context;
using SteamTogether.Core.Models;

namespace SteamTogether.Scraper.UnitTests;

public class GameCollectorTests
{
    private readonly HostBuilder _builder;
    
    public GameCollectorTests()
    {
        _builder = new HostBuilder();
        _builder
            .ConfigureServices((builder, services) =>
            {
                services
                    .AddLogging(x => x.AddConsole())
                    .AddDbContext<ApplicationDbContext>(
                        opt => { opt.UseInMemoryDatabase($"{nameof(GameCollectorTests)}.db"); });
            });
    }

    [Fact]
    public void Should_Return_Grouped_Games_ListTest()
    {
        var host = _builder.Build();
        var dbContext = host.Services.GetRequiredService<ApplicationDbContext>();

        var categories = new SteamGameCategory[]
        {
            new() {CategoryId = 1, Description = "Category1"},
            new() {CategoryId = 2, Description = "Category2"},
            new() {CategoryId = 3, Description = "Category3"},
            new() {CategoryId = 4, Description = "Category4"},
        };

        var games = new[]
        {
            new SteamGame
            {
                GameId = 1, SteamAppId = 1, Name = "Game1",
                Categories = new[]
                {
                    categories.First(c => c.CategoryId == 1),
                    categories.First(c => c.CategoryId == 2)
                }
            },
            new SteamGame
            {
                GameId = 2, SteamAppId = 2, Name = "Game2",
                Categories = new[] {categories.First(c => c.CategoryId == 1)}
            },
            new SteamGame
            {
                GameId = 3, SteamAppId = 3, Name = "Game3",
                Categories = new[] {categories.First(c => c.CategoryId == 3)}
            },

            new SteamGame
            {
                GameId = 4, SteamAppId = 4, Name = "Game4",
                Categories = new[]
                {
                    categories.First(c => c.CategoryId == 2),
                    categories.First(c => c.CategoryId == 3)
                }
            },
            new SteamGame
            {
                GameId = 5, SteamAppId = 5, Name = "Game5",
                Categories = new[] {categories.First(c => c.CategoryId == 2)}
            },
            new SteamGame
            {
                GameId = 6, SteamAppId = 6, Name = "Game6",
                Categories = new[]
                {
                    categories.First(c => c.CategoryId == 2),
                    categories.First(c => c.CategoryId == 3),
                    categories.First(c => c.CategoryId == 4)
                }
            }
        };

        var players = new[]
        {
            new SteamPlayer
            {
                Name = "Player1",
                PlayerId = 1,
                PlayerGames =
                {
                    new PlayerGame
                    {
                        GameId = 1,
                        PlaytimeForever = TimeSpan.FromHours(100),
                        Game = games.First(g => g.GameId == 1),
                    },
                    new PlayerGame
                    {
                        GameId = 2,
                        PlaytimeForever = TimeSpan.FromHours(35),
                        Game = games.First(g => g.GameId == 2),
                    },
                    new PlayerGame
                    {
                        GameId = 3,
                        PlaytimeForever = TimeSpan.FromHours(9900),
                        Game = games.First(g => g.GameId == 4),
                    },
                    new PlayerGame
                    {
                        GameId = 5,
                        PlaytimeForever = TimeSpan.FromHours(10),
                        Game = games.First(g => g.GameId == 5),
                    }
                }
            },
            new SteamPlayer
            {
                Name = "Player2",
                PlayerId = 2,
                PlayerGames =
                {
                    new PlayerGame
                    {
                        GameId = 1,
                        PlaytimeForever = TimeSpan.FromHours(50),
                        Game = games.First(g => g.GameId == 1),
                    },
                    new PlayerGame
                    {
                        GameId = 3,
                        PlaytimeForever = TimeSpan.FromHours(300),
                        Game = games.First(g => g.GameId == 2),
                    },
                    new PlayerGame
                    {
                        GameId = 3,
                        PlaytimeForever = TimeSpan.FromHours(1),
                        Game = games.First(g => g.GameId == 3),
                    }
                }
            },
        };

        var chats = new[]
        {
            new TelegramChatParticipant
            {
                ChatId = 1,
                SteamPlayer = players.First(p => p.PlayerId == 1),
                SteamPlayerId = 1,
                TelegramUserId = 1
            },
            new TelegramChatParticipant
            {
                ChatId = 1,
                SteamPlayer = players.First(p => p.PlayerId == 2),
                SteamPlayerId = 2,
                TelegramUserId = 2
            },
        };

        var polls = new[]
        {
            new TelegramPoll
            {
                ChatId = 1,
                MessageId = 1,
                PollId = "1",
                TelegramPollVotes = new[]
                {
                    new TelegramPollVote
                    {
                        PollId = "1",
                        TelegramUserId = 1,
                        PollVoteId = 1
                    },
                    new TelegramPollVote
                    {
                        PollId = "1",
                        TelegramUserId = 2,
                        PollVoteId = 2
                    }
                }
            }
        };

        dbContext.SteamGames.AddRange(games);
        dbContext.SteamPlayers.AddRange(players);
        dbContext.TelegramChatParticipants.AddRange(chats);
        dbContext.TelegramPolls.AddRange(polls);
        dbContext.SaveChanges();

        var collector = new GameCollector(dbContext);
        var result = collector.GetGroupGames(
            new long[] {1, 2}, 
            new uint[] {1, 2, 3, 4, 5, 6}, 
            5);
        
        Assert.Equal(5, result.Length);
        
        Assert.Equal("Game2", result[0].Name);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(2, result[0].PlayerNames.Length);
        Assert.Equal(603000, result[0].TotalSeconds.Average());
        Assert.True(result[0].PlayerNames.Any(p => p == "Player1"));
        Assert.True(result[0].PlayerNames.Any(p => p == "Player2"));
        
        Assert.Equal("Game1", result[1].Name);
        Assert.Equal(2, result[1].Count);
        Assert.Equal(2, result[1].PlayerNames.Length);
        Assert.Equal(270000, result[1].TotalSeconds.Average());
        Assert.True(result[1].PlayerNames.Any(p => p == "Player1"));
        Assert.True(result[1].PlayerNames.Any(p => p == "Player2"));
        
        Assert.Equal("Game4", result[2].Name);
        Assert.Equal(1, result[2].Count);
        Assert.Equal(1, result[2].PlayerNames.Length);
        Assert.Equal(35640000, result[2].TotalSeconds.Average());
        Assert.True(result[2].PlayerNames.Any(p => p == "Player1"));
        
        Assert.Equal("Game5", result[3].Name);
        Assert.Equal(1, result[3].Count);
        Assert.Equal(1, result[3].PlayerNames.Length);
        Assert.Equal(36000, result[3].TotalSeconds.Average());
        Assert.True(result[3].PlayerNames.Any(p => p == "Player1"));
        
        Assert.Equal("Game3", result[4].Name);
        Assert.Equal(1, result[4].Count);
        Assert.Equal(1, result[4].PlayerNames.Length);
        Assert.Equal(3600, result[4].TotalSeconds.Average());
        Assert.True(result[4].PlayerNames.Any(p => p == "Player2"));
    }
}
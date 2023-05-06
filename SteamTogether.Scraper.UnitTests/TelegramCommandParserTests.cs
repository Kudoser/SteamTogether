using SteamTogether.Bot.Services.Command.Parser;

namespace SteamTogether.Scraper.UnitTests;

public class TelegramCommandParserTests
{
    [Theory]
    [InlineData("/play Co-op", "play", new [] {"Co-op"})]
    [InlineData("/list", "list", new string[]{})]
    public void CommandAndArguments_Should_Be_ParsedTest(string input, string expectedCommand, string[] expectedArgs)
    {
        var sut = new TelegramCommandParser();
        var result = sut.Parse(input);
        
        Assert.Equal(expectedCommand, result.CommandName);
        Assert.Equal(expectedArgs, result.Arguments);
    }
}
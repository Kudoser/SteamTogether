using SteamTogether.Bot.Services.Command.Parser;

namespace SteamTogether.Scraper.UnitTests;

public class TelegramCommandParserTests
{
    [Theory]
    [InlineData("/play Co-op", "play", new [] {"Co-op"})]
    [InlineData("/list", "list", new string[]{})]
    [InlineData("/list    ", "list", new string[]{})]
    [InlineData("/test \"first\" \"second\"", "test", new[]{"first", "second"})]
    [InlineData("/test first second", "test", new[]{"first", "second"})]
    [InlineData("/test actually,one,argument,separated,by,comma", "test", new[]{"actually,one,argument,separated,by,comma"})]
    [InlineData("/test null", "test", new[]{"null"})]
    public void CommandAndArguments_Should_Be_ParsedTest(string input, string expectedCommand, string[] expectedArgs)
    {
        var sut = new TelegramCommandParser();
        var result = sut.Parse(input);
        
        Assert.True(result.Parsed);
        Assert.Equal(expectedCommand, result.CommandName);
        Assert.Equal(expectedArgs, result.Arguments);
    }
}
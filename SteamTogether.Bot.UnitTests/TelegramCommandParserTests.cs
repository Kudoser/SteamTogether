using SteamTogether.Bot.Services.Command.Parser;

namespace SteamTogether.Bot.UnitTests;

public class TelegramCommandParserTests
{
    [Theory]
    [InlineData("/list", "botname", "list")]
    [InlineData("/join", "botname", "join")]
    [InlineData("/join@botname", "botname", "join")]
    public void Command_ShouldBe_ParsedTest(string input, string botName, string expected)
    {
        var sut = new TelegramCommandParser();

        var result = sut.Parse(input, botName);
        Assert.Equal(expected, result.CommandName);
    }

    [Theory]
    [InlineData("list")]
    [InlineData("test")]
    public void Command_Should_ThrowException_IfCommandDoesNotStartWithSlashTest(string input)
    {
        var sut = new TelegramCommandParser();
        var result = sut.Parse(input, "testbot");

        Assert.False(result.Parsed);
    }
    
    [Theory]
    [InlineData("/play Co-op", "play", new [] {"Co-op"})]
    [InlineData("/play@botname Co-op", "play", new [] {"Co-op"})]
    [InlineData("/list", "list", new string[]{})]
    [InlineData("/list    ", "list", new string[]{})]
    [InlineData("/test \"first\" \"second\"", "test", new[]{"first", "second"})]
    [InlineData("/test first second", "test", new[]{"first", "second"})]
    [InlineData("/test actually,one,argument,separated,by,comma", "test", new[]{"actually,one,argument,separated,by,comma"})]
    [InlineData("/test null", "test", new[]{"null"})]
    [InlineData("/test@botname null", "test", new[]{"null"})]
    public void CommandAndArguments_Should_Be_ParsedTest(string input, string expectedCommand, string[] expectedArgs)
    {
        var sut = new TelegramCommandParser();
        var result = sut.Parse(input,"botname");
        
        Assert.True(result.Parsed);
        Assert.Equal(expectedCommand, result.CommandName);
        Assert.Equal(expectedArgs, result.Arguments);
    }
}

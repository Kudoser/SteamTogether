using SteamTogether.Bot.Services.Command.Parser;
using SteamTogether.Core.Exceptions;

namespace SteamTogether.Bot.UnitTests;

public class TelegramCommandParserTests
{
    [Theory]
    [InlineData("/list", "list")]
    [InlineData("/join", "join")]
    public void Command_ShouldBe_ParsedTest(string input, string expected)
    {
        var sut = new TelegramCommandParser();

        var result = sut.Parse(input);
        Assert.Equal(expected, result.CommandName);
    }

    [Theory]
    [InlineData("list")]
    [InlineData("test")]
    public void Command_Should_ThrowException_IfCommandDoesNotStartWithSlashTest(string input)
    {
        var sut = new TelegramCommandParser();
        Assert.Throws<ParseCommandException>(() => sut.Parse(input));
    }
}

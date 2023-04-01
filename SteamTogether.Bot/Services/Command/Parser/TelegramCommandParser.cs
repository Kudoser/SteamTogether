namespace SteamTogether.Bot.Services.Command.Parser;

public class TelegramCommandParser : ITelegramCommandParser
{
    public ParseResult Parse(string input)
    {
        if (input.StartsWith("/"))
        {
        }

        return new ParseResult
        {
            CommandName = "list",
            Arguments = new[]
            {
                "1"
            }
        };
    }
}
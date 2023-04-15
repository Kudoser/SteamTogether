namespace SteamTogether.Bot.Services.Command.Parser;

public class TelegramCommandParser : ITelegramCommandParser
{
    public ParseResult Parse(string input)
    {
        var result = new ParseResult();
        if (!input.StartsWith("/"))
        {
            return result;
        }

        var parts = input.Split(' ');
        result.CommandName = parts[0][1..];
        result.Arguments = parts.Length > 1 ? parts[1].Split(',') : Array.Empty<string>();
        result.Parsed = true;

        return result;
    }
}
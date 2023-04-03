using SteamTogether.Bot.Exceptions;

namespace SteamTogether.Bot.Services.Command.Parser;

public class TelegramCommandParser : ITelegramCommandParser
{
    public ParseResult Parse(string input)
    {
        if (!input.StartsWith("/"))
        {
            throw new ParseCommandException(
                $"Error occured in attempt to parse command input: {input}"
            );
        }

        var parts = input.Split(' ');
        var command = parts[0][1..];

        var arguments = parts.Length > 1 ? parts[1].Split(',') : Array.Empty<string>();

        return new ParseResult { CommandName = command, Arguments = arguments };
    }
}

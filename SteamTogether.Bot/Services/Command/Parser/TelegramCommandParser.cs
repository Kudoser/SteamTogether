using Microsoft.CodeAnalysis;

namespace SteamTogether.Bot.Services.Command.Parser;

public class TelegramCommandParser : ITelegramCommandParser
{
    public ParseResult Parse(string input, string botName)
    {
        var result = new ParseResult();
        if (!input.StartsWith("/"))
        {
            return result;
        }

        var parts = input.Split(' ');
        var commandNameWithBackslash = parts[0];
        var commandArgumentsAsString = input.Substring(commandNameWithBackslash.Length);

        var fullBotName = $"@{botName}";
        var index = commandNameWithBackslash.IndexOf(fullBotName, StringComparison.InvariantCulture);
        var clearedCommandName = index < 0
            ? commandNameWithBackslash
            : commandNameWithBackslash.Remove(index, fullBotName.Length);
        
        result.CommandName = clearedCommandName[1..];
        result.Arguments = parts.Length > 1
            ? CommandLineParser.SplitCommandLineIntoArguments(commandArgumentsAsString, true).ToArray()
            : new string[] { };
        
        result.Parsed = true;

        return result;
    }
}

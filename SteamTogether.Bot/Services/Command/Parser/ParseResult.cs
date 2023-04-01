namespace SteamTogether.Bot.Services.Command.Parser;

public class ParseResult
{
    public string CommandName { get; set; }
    public IEnumerable<string> Arguments { get; set; }
}
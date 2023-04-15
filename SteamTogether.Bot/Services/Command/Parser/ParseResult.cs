namespace SteamTogether.Bot.Services.Command.Parser;

public class ParseResult
{
    public string CommandName { get; set; } = default!;
    public string[] Arguments { get; set; } = default!;
    public bool Parsed { get; set; }
}

namespace SteamTogether.Bot.Services.Command.Parser;

public interface ITelegramCommandParser
{
    public ParseResult Parse(string input);
}

namespace SteamTogether.Bot.Exceptions;

public class ParseCommandException : Exception
{
    public ParseCommandException(string? message) : base(message)
    {
    }
}
namespace SteamTogether.Bot.Exceptions;

public class UnknownCommandException : Exception
{
    public UnknownCommandException(string? message)
        : base(message) { }
}

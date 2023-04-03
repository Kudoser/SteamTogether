namespace SteamTogether.Core.Exceptions;

public class ParseCommandException : Exception
{
    public ParseCommandException(string? message)
        : base(message) { }
}

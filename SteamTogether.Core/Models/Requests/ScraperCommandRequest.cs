namespace SteamTogether.Core.Models.Requests;

public class ScraperCommandRequest
{
    public CommandRequest Command { get; set; }
    public string[] Arguments { get; set; } = { };
}
namespace SteamTogether.Core.Models.Responses;

public class ScraperStatusResponse : ScraperCommandResponse
{
    public ScraperSyncStatus Status { get; set; }
}
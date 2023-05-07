namespace SteamTogether.Scraper.Services;

public interface IHttpCommandListener
{
    Task StartAsync();
    Task ReceiveAsync(IScraperService scraper);
}

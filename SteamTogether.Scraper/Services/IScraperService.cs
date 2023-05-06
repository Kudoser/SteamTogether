using SteamTogether.Core.Models;

namespace SteamTogether.Scraper.Services;

public interface IScraperService
{
    public Task RunSync();
    ScraperSyncStatus SyncStatus { get;  }
}

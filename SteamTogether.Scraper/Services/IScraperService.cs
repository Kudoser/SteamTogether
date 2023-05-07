using SteamTogether.Core.Models;

namespace SteamTogether.Scraper.Services;

public interface IScraperService
{
    public Task RunSync();
    public Task RunSync(ulong[] playerIds);
    ScraperSyncStatus SyncStatus { get;  }
}

using Steam.Models.SteamCommunity;
using Steam.Models.SteamStore;
using SteamWebAPI2.Utilities;

namespace SteamTogether.Core.Services.Steam;

public interface ISteamService
{
    public Task<ISteamWebResponse<OwnedGamesResultModel>> GetOwnedGamesAsync(ulong playerId);
    public Task<StoreAppDetailsDataModel> GetAppDetailsAsync(uint gameId);
    public Task<ISteamWebResponse<PlayerSummaryModel>> GetPlayerSummaryAsync(ulong playerId);
}

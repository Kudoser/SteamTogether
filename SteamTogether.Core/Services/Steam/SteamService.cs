using Microsoft.Extensions.Options;
using Steam.Models.SteamCommunity;
using Steam.Models.SteamStore;
using SteamTogether.Core.Options;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace SteamTogether.Core.Services.Steam;

public class SteamService : ISteamService
{
    private readonly SteamWebInterfaceFactory _steamInterfaceFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    public SteamService(IOptions<SteamOptions> options, IHttpClientFactory httpClientFactory)
    {
        _steamInterfaceFactory = new SteamWebInterfaceFactory(options.Value.ApiKey);
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ISteamWebResponse<OwnedGamesResultModel>> GetOwnedGamesAsync(ulong playerId)
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(PlayerService));
        var playerInterface = _steamInterfaceFactory.CreateSteamWebInterface<PlayerService>(httpClient);
        return await playerInterface.GetOwnedGamesAsync(playerId);
    }

    public async Task<StoreAppDetailsDataModel> GetAppDetailsAsync(uint gameId)
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(SteamStore));
        var storeInterface = _steamInterfaceFactory.CreateSteamStoreInterface(httpClient);
        return await storeInterface.GetStoreAppDetailsAsync(gameId, "", "en");
    }

    public async Task<ISteamWebResponse<PlayerSummaryModel>> GetPlayerSummaryAsync(ulong playerId)
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(SteamUser));
        var userInterface = _steamInterfaceFactory.CreateSteamWebInterface<SteamUser>(httpClient);
        return await userInterface.GetPlayerSummaryAsync(playerId);
    }
}
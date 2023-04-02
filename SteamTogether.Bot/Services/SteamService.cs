using Microsoft.Extensions.Options;
using SteamTogether.Bot.Options;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace SteamTogether.Bot.Services;

public class SteamService : ISteamService
{
    private readonly ISteamWebInterfaceFactory _steamFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    public SteamService(IOptions<SteamOptions> options, IHttpClientFactory httpClientFactory)
    {
        _steamFactory = new SteamWebInterfaceFactory(options.Value.ApiKey);
        _httpClientFactory = httpClientFactory;
    }

    public SteamUser GetSteamUserWebInterface()
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(SteamUser));
        return _steamFactory.CreateSteamWebInterface<SteamUser>(httpClient);
    }
}

using Microsoft.Extensions.Options;
using SteamTogether.Core.Options;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace SteamTogether.Core.Services.Steam;

public class SteamService : ISteamService
{
    private readonly ISteamWebInterfaceFactory _steamFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    public SteamService(IOptions<SteamOptions> options, IHttpClientFactory httpClientFactory)
    {
        _steamFactory = new SteamWebInterfaceFactory(options.Value.ApiKey);
        _httpClientFactory = httpClientFactory;
    }

    public T GetSteamUserWebInterface<T>()
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(T));
        return _steamFactory.CreateSteamWebInterface<T>(httpClient);
    }

    public SteamStore CreateSteamStoreInterface()
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(SteamStore));
        return _steamFactory.CreateSteamStoreInterface(httpClient);
    }
}

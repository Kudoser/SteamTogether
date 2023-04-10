using SteamWebAPI2.Interfaces;

namespace SteamTogether.Core.Services.Steam;

public interface ISteamService
{
    T GetSteamUserWebInterface<T>();
    public SteamStore CreateSteamStoreInterface();
}

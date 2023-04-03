using SteamWebAPI2.Interfaces;

namespace SteamTogether.Core.Services.Steam;

public interface ISteamService
{
    public SteamUser GetSteamUserWebInterface();
}

using SteamWebAPI2.Interfaces;

namespace SteamTogether.Bot.Services;

public interface ISteamService
{
    public SteamUser GetSteamUserWebInterface();
}
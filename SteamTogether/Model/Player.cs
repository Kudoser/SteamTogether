using System.Collections.Generic;
using Steam.Models.SteamCommunity;

namespace SteamTogether.Model
{
    public class Player
    {
        public IEnumerable<OwnedGameModel> OwnedGames;
        public PlayerSummaryModel Info;
    }
}
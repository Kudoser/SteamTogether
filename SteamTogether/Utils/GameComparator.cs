using System.Collections.Generic;
using Steam.Models.SteamCommunity;

namespace SteamTogether.Utils
{
    internal class GameComparator : IEqualityComparer<OwnedGameModel>
    {
        public bool Equals(OwnedGameModel x, OwnedGameModel y)
        {
            return x.AppId == y.AppId;
        }

        public int GetHashCode(OwnedGameModel obj)
        {
            return obj.AppId.GetHashCode();
        }
    }
}
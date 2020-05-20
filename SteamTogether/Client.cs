using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steam.Models.SteamCommunity;
using SteamTogether.Model;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace SteamTogether
{
    public class Client
    {
        private readonly PlayerService _steamPlayerInterface;
        private readonly SteamUser _steamUserInterface;

        public Client(string steamApiKey)
        {
            var webInterfaceFactory = new SteamWebInterfaceFactory(steamApiKey);
            _steamPlayerInterface = webInterfaceFactory.CreateSteamWebInterface<PlayerService>();
            _steamUserInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>();
        }

        public async Task<Dictionary<long, Player>> GetUsersInfo(IEnumerable<long> steamIds)
        {
            var users = new Dictionary<long, Player>();
            var tasks = new List<Task>();
            foreach (var steamId in steamIds)
            {
                var infoTask = GetUserInfo(steamId);
                var gamesTask = GetOwnedGames(steamId);
                users.Add(
                    steamId,
                    new Player()
                    {
                        Info = infoTask.Result,
                        OwnedGames = gamesTask.Result
                    });
                
                tasks.Add(infoTask);
                tasks.Add(gamesTask);
            }

            await Task.WhenAll(tasks);

            return users;
        }

        private async Task<List<OwnedGameModel>> GetOwnedGames(long steamId)
        {
            var id = Convert.ToUInt64(steamId);
            var response = await _steamPlayerInterface.GetOwnedGamesAsync(id, includeAppInfo: true);
            return response.Data.OwnedGames.ToList();
        }

        private async Task<PlayerSummaryModel> GetUserInfo(long steamId)
        {
            var id = Convert.ToUInt64(steamId);
            var response = await _steamUserInterface.GetPlayerSummaryAsync(id);
            return response.Data;
        }
    }
}
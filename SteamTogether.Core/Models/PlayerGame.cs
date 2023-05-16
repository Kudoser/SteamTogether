using System.ComponentModel.DataAnnotations.Schema;

namespace SteamTogether.Core.Models;

public class PlayerGame
{
    [ForeignKey(nameof(SteamGame))] 
    public uint GameId { get; set; }
    
    [ForeignKey(nameof(SteamPlayer))]
    public ulong PlayerId { get; set; }
    public TimeSpan PlaytimeForever { get; set; }
    public TimeSpan? PlaytimeLastTwoWeeks { get; set; }
    
    public SteamPlayer Player { get; set; }
    public SteamGame Game { get; set; }
}
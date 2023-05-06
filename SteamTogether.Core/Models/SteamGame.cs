using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Core.Models;

public class SteamGame
{
    [Key]
    public uint GameId { get; set; }
    public uint SteamAppId { get; set; }
    public string? Name { get; set; }

    public bool Multiplayer { get; set; }

    public DateTime? LastSyncDateTime { get; set; }
    public ICollection<SteamPlayer> Players { get; } = new List<SteamPlayer>();
    public ICollection<SteamGameCategory> Categories { get; set; } = new List<SteamGameCategory>();
}

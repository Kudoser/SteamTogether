using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Core.Models;

public class SteamGame
{
    [Key]
    public uint GameId { get; set; }
    public uint SteamAppId { get; set; }
    [Required]
    public string? Name { get; set; }
    public DateTime? LastSyncDateTime { get; set; }
    public ICollection<PlayerGame> PlayerGames { get; } = new List<PlayerGame>();
    public ICollection<SteamGameCategory> Categories { get; set; } = new List<SteamGameCategory>();
}

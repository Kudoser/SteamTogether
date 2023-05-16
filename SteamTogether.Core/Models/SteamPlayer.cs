using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamTogether.Core.Models;

[Table("SteamPlayers")]
public class SteamPlayer
{
    [Key]
    public ulong PlayerId { get; set; }
    public string? Name { get; set; }

    public string? ApiKey { get; set; }
    public DateTime? LastSyncDateTime { get; set; }
    public ICollection<PlayerGame> PlayerGames { get; } = new List<PlayerGame>();
}

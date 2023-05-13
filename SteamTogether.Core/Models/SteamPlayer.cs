using System.ComponentModel.DataAnnotations.Schema;

namespace SteamTogether.Core.Models;

[Table("SteamPlayers")]
public class SteamPlayer
{
    public ulong PlayerId { get; set; }
    public string? Name { get; set; }

    public string? ApiKey { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime? LastSyncDateTime { get; set; }
    public ICollection<SteamGame> Games { get; } = new List<SteamGame>();
}

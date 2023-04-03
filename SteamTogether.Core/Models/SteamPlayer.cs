using System.ComponentModel.DataAnnotations.Schema;

namespace SteamTogether.Core.Models;

[Table("SteamPlayers")]
public class SteamPlayer
{
    public ulong PlayerId { get; set; }

    public string? ApiKey { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime LasUpdated { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime Inserted { get; set; }
    public DateTime LastSyncDateTime { get; set; }
    public ICollection<TelegramChat> TelegramChats { get; } = new List<TelegramChat>();
}

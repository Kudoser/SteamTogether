using System.ComponentModel.DataAnnotations.Schema;

namespace SteamTogether.Bot.Models;

[Table("SteamPlayers")]
public class SteamPlayer
{
    public ulong PlayerId { get; set; }

    public string? ApiKey { get; set; }
    public ICollection<TelegramChat> TelegramChats { get; } = new List<TelegramChat>();
}
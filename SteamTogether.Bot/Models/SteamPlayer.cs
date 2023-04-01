using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamTogether.Bot.Models;

[Table("SteamPlayers")]
public class SteamPlayer
{
    [Key] 
    public string PlayerId { get; set; }
    
    public string? ApiKey { get; set; }
}
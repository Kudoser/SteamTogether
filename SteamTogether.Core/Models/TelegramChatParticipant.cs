using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamTogether.Core.Models;

[Table("TelegramChatParticipants")]
public class TelegramChatParticipant
{
    [Required]
    public long ChatId { get; set; }
    
    [Required]
    public ulong SteamPlayerId  { get; set; }
    [Required]
    public long TelegramUserId { get; set; }
    
    public SteamPlayer? SteamPlayer { get; }
}

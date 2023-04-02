namespace SteamTogether.Bot.Models;

public class TelegramChat
{
    public long ChatId { get; set; }
    
    public ICollection<SteamPlayer> Players { get; set; } = new List<SteamPlayer>();
}
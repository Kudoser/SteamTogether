namespace SteamTogether.Bot.Models;

public class TelegramChat
{
    public int ChatId { get; set; }
    
    public IEnumerable<SteamPlayer> Players { get; set; } = new List<SteamPlayer>();
}
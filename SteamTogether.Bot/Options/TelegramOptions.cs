using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Bot.Options;

public class TelegramOptions
{
    public const string Telegram = "Telegram";

    [Required]
    public string Token { get; set; } = default!;
}

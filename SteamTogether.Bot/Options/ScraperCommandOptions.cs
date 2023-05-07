using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Bot.Options;

public class ScraperCommandOptions
{
    public const string ScraperCommand = "ScraperCommand";

    [Required]
    public string Url { get; set; } = default!;
}

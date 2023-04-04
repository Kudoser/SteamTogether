using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Scraper.Options;

public class ScraperOptions
{
    public const string Scraper = "Scraper";

    [Required]
    public string Schedule { get; init; } = default!;
    public bool RunOnStartup { get; init; }
}

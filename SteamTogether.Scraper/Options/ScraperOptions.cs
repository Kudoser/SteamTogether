using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Scraper.Options;

public class ScraperOptions
{
    public const string Scraper = "Scraper";

    [Required]
    public string Schedule { get; init; } = default!;
    public bool RunOnStartup { get; init; }
    public int SyncPeriodSeconds { get; set; } = 18000;
    public int PlayersPerRun { get; set; } = 10;
}

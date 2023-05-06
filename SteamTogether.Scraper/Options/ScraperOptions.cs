using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Scraper.Options;

public class ScraperOptions
{
    public const string Scraper = "Scraper";

    [Required]
    public string Schedule { get; set; } = default!;
    public bool RunOnStartup { get; set; }
    public int PlayerSyncPeriodSeconds { get; set; } = 18000;
    public int GamesSyncPeriodMinutes { get; set; } = 1440;
    public int PlayersPerRun { get; set; } = 10;

    [Required]
    public int HttpCommandPort { get; init; }
}

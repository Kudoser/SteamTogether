using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Bot.Options;

public class ScraperClientOptions
{
    public const string ScraperClient = "ScraperClient";

    [Required] public string Url { get; init; }

    public bool Enabled { get; init; }
}
using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Scraper.Options;

public class HttpServerOptions
{
    [Required] public string Url { get; init; } = default!;
    public bool Enabled { get; init; }
}
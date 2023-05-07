using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Scraper.Options;

public class HttpServerOptions
{
    [Required]
    public int Port { get; init; }
    public bool Enabled { get; init; }
}
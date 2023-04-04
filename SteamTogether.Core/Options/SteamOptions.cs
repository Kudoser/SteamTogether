using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Core.Options;

public class SteamOptions
{
    public const string Steam = "Steam";

    [Required]
    public string ApiKey { get; set; } = default!;
}

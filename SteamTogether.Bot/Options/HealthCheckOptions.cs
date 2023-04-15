using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Bot.Options;

public class HealthCheckOptions
{
    public const string HealthCheck = "HealthCheck";
    
    [Required]
    public int Port { get; init; }
}
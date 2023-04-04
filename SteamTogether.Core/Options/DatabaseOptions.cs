using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Core.Options;

public class DatabaseOptions
{
    public const string Database = "Database";

    [Required]
    public string ConnectionString { get; set; } = default!;
}

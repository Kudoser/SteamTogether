namespace SteamTogether.Bot.Options;

public class DatabaseOptions
{
    public const string Database = "Database";

    public string ConnectionString { get; set; } = default!;
    public SeedDataOptions SeedData { get; set; } = default!;
}

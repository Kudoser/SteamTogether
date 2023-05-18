namespace SteamTogether.Bot.Models;

public class GroupedGameResult
{
    public string Name { get; set; } = default!;
    public int Count { get; set; }
    public string[] PlayerNames { get; set; } = { };
    public double[] TotalSeconds { get; set; } = default!;
}
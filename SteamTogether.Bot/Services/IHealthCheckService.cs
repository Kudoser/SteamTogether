namespace SteamTogether.Bot.Services;

public interface IHealthCheckService
{
    Task StartAsync();
    Task ReceiveAsync();
}
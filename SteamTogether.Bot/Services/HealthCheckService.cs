using System.Net;
using Microsoft.Extensions.Options;
using SteamTogether.Bot.Options;

namespace SteamTogether.Bot.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly HealthCheckOptions _options;
    private readonly ILogger<HealthCheckService> _logger;
    private readonly HttpListener _httpListener;

    public HealthCheckService(IOptions<HealthCheckOptions> options, ILogger<HealthCheckService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _httpListener = new HttpListener();
    }

    public Task StartAsync()
    {
        var url = $"http://*:{_options.Port}/";
        _httpListener.Prefixes.Add(url);

        _logger.LogInformation("Start listening on {Url}", url);
        _httpListener.Start();

        return Task.CompletedTask;
    }

    public async Task ReceiveAsync()
    {
        _logger.LogInformation("Health check message received");
        var context = await _httpListener.GetContextAsync();
        var response = context.Response;
        response.StatusCode = (int) HttpStatusCode.OK;
        response.OutputStream.Close();
        response.Close();
        _logger.LogInformation("Response HTTP 200 sent");
    }
}
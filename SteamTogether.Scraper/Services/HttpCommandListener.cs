using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SteamTogether.Core.Models.Responses;
using SteamTogether.Scraper.Options;

namespace SteamTogether.Scraper.Services;

public class HttpCommandListener : IHttpCommandListener
{
    private readonly ScraperOptions _options;
    private readonly ILogger<HttpCommandListener> _logger;
    private readonly HttpListener _httpListener;

    public HttpCommandListener(
        IOptions<ScraperOptions> options,
        ILogger<HttpCommandListener> logger
    )
    {
        _options = options.Value;
        _logger = logger;
        _httpListener = new HttpListener();
    }

    public Task StartAsync()
    {
        var url = $"http://*:{_options.HttpCommandPort}/";
        _httpListener.Prefixes.Add(url);

        _logger.LogInformation("Start listening http commands on {Url}", url);
        _httpListener.Start();

        return Task.CompletedTask;
    }

    public async Task ReceiveAsync(IScraperService scraper)
    {
        var context = await _httpListener.GetContextAsync();
        if (context == null)
        {
            return;
        }
        
        // @todo parse commands

        _logger.LogInformation("status={Status}", scraper.SyncStatus);
        using var response = context.Response;
        var scraperResponse = new ScraperStatusResponse { Status = scraper.SyncStatus};
        
        response.Headers.Set("Content-Type", "application/json");
        var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(scraperResponse));
        response.ContentLength64 = buffer.Length;
        response.StatusCode = (int)HttpStatusCode.OK;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Close();
        response.Close();

        _logger.LogInformation("Response HTTP 200 sent");
    }
}

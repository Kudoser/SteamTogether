using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SteamTogether.Core.Models;
using SteamTogether.Core.Models.Requests;
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
        
        await using var receiveStream = context.Request.InputStream;
        using var readStream = new StreamReader(receiveStream, Encoding.UTF8);
        var content = await readStream.ReadToEndAsync();

        ScraperStatusCommandRequest? commandRequest;
        try
        {
            commandRequest = JsonSerializer.Deserialize<ScraperStatusCommandRequest>(content);
            ArgumentNullException.ThrowIfNull(commandRequest);
        }
        catch (Exception)
        {
            await RespondWithStatus(
                context.Response, 
                new ScraperErrorResponse {Error = "Unknown command"},
                HttpStatusCode.BadRequest);
            
            return;
        }

        if (commandRequest.Command == CommandRequest.Status)
        {
            var currentStatus = scraper.SyncStatus;
            _logger.LogInformation("Status command requested, current status: {Status}", currentStatus);
            var scraperResponse = new ScraperStatusResponse { Status = currentStatus };
            await RespondWithStatus(context.Response, scraperResponse, HttpStatusCode.OK);
            return;
        }
        
        var errorResponse = new ScraperErrorResponse { Error = "Unknown command" };
        await RespondWithStatus(context.Response, errorResponse, HttpStatusCode.NotFound);
    }

    private static async Task RespondWithStatus(HttpListenerResponse response, object model, HttpStatusCode code)
    {
        await RespondWithContent(response, model);
        response.StatusCode = (int)code;
    }

    private static async Task RespondWithContent(HttpListenerResponse response, object model)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(model));
        response.ContentLength64 = bytes.Length;
        response.Headers.Set("Content-Type", "application/json");
        await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        response.OutputStream.Close();
        response.Close();
    }
}

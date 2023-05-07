using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SteamTogether.Bot.Options;
using SteamTogether.Core.Models;
using SteamTogether.Core.Models.Requests;
using SteamTogether.Core.Models.Responses;

namespace SteamTogether.Bot.Services;

public class ScraperCommandHttpClient : IScraperCommandClient
{
    private readonly ScraperClientOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public ScraperCommandHttpClient(IOptions<ScraperClientOptions> options, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<ScraperCommandResponse> RequestSyncAsync(string[] playerIds)
    {
        if (!_options.Enabled)
        {
            return new ScraperCommandResponse
            {
                Success = false,
                Message = "Http server is disabled"
            };
        }
        
        var httpClient = _httpClientFactory.CreateClient(nameof(ScraperCommandHttpClient));
        var command = new ScraperCommandRequest {Command = CommandRequest.Sync, Arguments = playerIds};
        
        using var jsonContent = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
        var result = await httpClient.PostAsync(_options.Url, jsonContent);
        var content = await result.Content.ReadAsStringAsync();
        
        var commandResponse = JsonSerializer.Deserialize<ScraperCommandResponse>(content);
        ArgumentNullException.ThrowIfNull(commandResponse);
        
        return commandResponse;
    }

    public async Task<ScraperStatusResponse> RequestStatusAsync()
    {
        if (!_options.Enabled)
        {
            return new ScraperStatusResponse
            {
                Success = false,
                Message = "Http server is disabled"
            };
        }
        
        var httpClient = _httpClientFactory.CreateClient(nameof(ScraperCommandHttpClient));
        var command = new ScraperCommandRequest {Command = CommandRequest.Status};
        
        using var jsonContent = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
        var result = await httpClient.PostAsync(_options.Url, jsonContent);
        var content = await result.Content.ReadAsStringAsync();
        
        var commandResponse = JsonSerializer.Deserialize<ScraperStatusResponse>(content);
        ArgumentNullException.ThrowIfNull(commandResponse);
        
        return commandResponse;
    }
}

public interface IScraperCommandClient
{
    public Task<ScraperCommandResponse> RequestSyncAsync(string[] playerIds);
    public Task<ScraperStatusResponse> RequestStatusAsync();
}
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
    private readonly ScraperCommandOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public ScraperCommandHttpClient(IOptions<ScraperCommandOptions> options, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<ScraperCommandResponse> RequestSyncAsync(string[] playerIds)
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(ScraperCommandHttpClient));
        var command = new ScraperCommandRequest {Command = CommandRequest.Sync, Arguments = playerIds};
        
        using var jsonContent = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
        var result = await httpClient.PostAsync($"{_options.Url}:{_options.Port}", jsonContent);
        var content = await result.Content.ReadAsStringAsync();
        
        var commandResponse = JsonSerializer.Deserialize<ScraperCommandResponse>(content);
        ArgumentNullException.ThrowIfNull(commandResponse);
        
        return commandResponse;
    }

    public Task RequestStatusAsync()
    {
        throw new NotImplementedException();
    }
}

public interface IScraperCommandClient
{
    public Task<ScraperCommandResponse> RequestSyncAsync(string[] playerIds);
    public Task RequestStatusAsync();
}
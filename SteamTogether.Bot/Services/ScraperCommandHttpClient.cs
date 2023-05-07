using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SteamTogether.Bot.Options;
using SteamTogether.Core.Models;
using SteamTogether.Core.Models.Requests;

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

    public async Task<string> RequestSync(string[] playerIds)
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(ScraperCommandHttpClient));
        var command = new ScraperCommandRequest {Command = CommandRequest.Sync, Arguments = playerIds};
        
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(command),
            Encoding.UTF8,
            "application/json");
        var result = await httpClient.PostAsync($"http://localhost:{_options.Port}", jsonContent);

        result.EnsureSuccessStatusCode();
        return await result.Content.ReadAsStringAsync();
    }

    public Task RequestStatus()
    {
        throw new NotImplementedException();
    }
}

public interface IScraperCommandClient
{
    public Task<string> RequestSync(string[] playerIds);
    public Task RequestStatus();
}
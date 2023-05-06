using System.Text.Json.Serialization;

namespace SteamTogether.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CommandRequest
{
    [JsonPropertyName("Status")]
    Status,
    [JsonPropertyName("Sync")]
    Sync
}
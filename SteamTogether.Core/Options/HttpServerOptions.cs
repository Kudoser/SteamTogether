namespace SteamTogether.Core.Options;

public class HttpServerOptions
{
    public const string HttpServer = "HttpServer";
    
    public string Url { get; init; } = default!;
    public bool Enabled { get; set; }
    public int Port { get; set; } = default!;
}
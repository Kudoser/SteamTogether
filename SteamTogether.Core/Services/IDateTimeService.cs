namespace SteamTogether.Core.Services;

/// <summary>
/// Interface for a service that provides the current UTC date and time.
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    public DateTime UtcNow { get; }
}

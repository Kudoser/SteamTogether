namespace SteamTogether.Core.Services;

public class DateTimeService : IDateTimeService
{
    private readonly DateTime? _now;

    public DateTimeService(DateTime? now = null)
    {
        _now = now;
    }

    public DateTime UtcNow
    {
        get => _now ?? DateTime.UtcNow;
    }
}

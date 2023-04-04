namespace SteamTogether.Bot.Services;

public class DateTimeService : IDateTimeService
{
    private readonly DateTime _now;

    public DateTimeService(DateTime? now = null)
    {
        _now = now ?? DateTime.UtcNow;
    }

    public DateTime GetCurrentTime()
    {
        return _now;
    }
}

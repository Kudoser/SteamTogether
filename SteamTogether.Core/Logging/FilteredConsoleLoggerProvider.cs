using Microsoft.Extensions.Logging;

namespace SteamTogether.Core.Logging;

public class FilteredConsoleLoggerProvider : ILoggerProvider
{
    private readonly Func<string, LogLevel, bool> _filter;
    private readonly bool _filterSensitiveData;

    public FilteredConsoleLoggerProvider(
        Func<string, LogLevel, bool> filter,
        bool filterSensitiveData = false
    )
    {
        _filter = filter;
        _filterSensitiveData = filterSensitiveData;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FilteredConsoleLogger(categoryName, _filter, _filterSensitiveData);
    }

    public void Dispose() { }
}

public class FilteredConsoleLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Func<string, LogLevel, bool> _filter;
    private readonly bool _filterSensitiveData;

    public FilteredConsoleLogger(
        string categoryName,
        Func<string, LogLevel, bool> filter,
        bool filterSensitiveData
    )
    {
        _categoryName = categoryName;
        _filter = filter;
        _filterSensitiveData = filterSensitiveData;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel)
    {
        return _filter(_categoryName, logLevel);
    }

    void ILogger.Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (_filterSensitiveData)
        {
            state = (TState)FilterSensitiveData(state);
        }

        Console.WriteLine($"{logLevel}: {formatter(state, exception)}");
    }

    private static object FilterSensitiveData<TState>(TState state)
    {
        if (state is string)
        {
            return FilterStringSensitiveData((string)(object)state);
        }

        return state;
    }

    private static string FilterStringSensitiveData(string message)
    {
        // Replace any occurrences of "password" with "***"
        return message.Replace("password", "***");
    }
}

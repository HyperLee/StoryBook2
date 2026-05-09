using Microsoft.Extensions.Logging;

namespace StoryBook.Tests.Support;

internal sealed class RecordingLogger<T> : ILogger<T>
{
    private readonly List<LogEntry> _entries = [];

    public IReadOnlyList<LogEntry> Entries => _entries;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        _entries.Add(new LogEntry(logLevel, eventId, formatter(state, exception), exception));
    }

    public bool Contains(LogLevel level, string text)
    {
        return _entries.Any(entry =>
            entry.Level == level
            && entry.Message.Contains(text, StringComparison.OrdinalIgnoreCase));
    }
}

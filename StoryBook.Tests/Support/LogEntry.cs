using Microsoft.Extensions.Logging;

namespace StoryBook.Tests.Support;

internal sealed record LogEntry(
    LogLevel Level,
    EventId EventId,
    string Message,
    Exception? Exception);

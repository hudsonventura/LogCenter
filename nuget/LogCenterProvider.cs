using Microsoft.Extensions.Logging;

namespace nuget;

public class LogCenterProvider : ILoggerProvider
{
    LogCenterSinkOptions _options;
    public LogCenterProvider(LogCenterSinkOptions options)
    {
        _options = options;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new LogCenterSink(_options);
    }

    public void Dispose()
    {
        // Limpeza de recursos, se necessário
    }
}

using Serilog;

namespace nuget;

public static class LoggerConfig
{
    public static LoggerConfiguration CreateLogger(LogCenterSinkOptions options)
    {
        return new LoggerConfiguration();
    }
}
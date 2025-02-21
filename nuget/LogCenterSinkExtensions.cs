using Serilog;
using Serilog.Configuration;

namespace nuget;

public static class LogCenterSinkExtensions
{
    public static LoggerConfiguration LogCenter(this LoggerSinkConfiguration loggerConfiguration, LogCenterSinkOptions options)
    {
        return loggerConfiguration.Sink(new LogCenterSink(options));
    }
}
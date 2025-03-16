namespace LogCenter;

public interface ILogger
{
    void Log(LogLevel level, string message, dynamic data = null);
    void Log(LogLevel level, string message, Guid traceId, dynamic data = null);
    void Log(LogLevel level, string message, string traceId, dynamic data = null);
    
    Task LogAsync(LogLevel level, string message, dynamic data = null);
    Task LogAsync(LogLevel level, string message, Guid traceId, dynamic data = null);
    Task LogAsync(LogLevel level, string message, string traceId, dynamic data = null);
}

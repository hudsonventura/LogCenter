namespace LogCenter;

public interface ILogger
{
    void Log(LogLevel level, string message, dynamic data = null);
    
    Task LogAsync(LogLevel level, string message, dynamic data = null);
}

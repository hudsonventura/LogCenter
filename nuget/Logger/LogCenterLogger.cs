
namespace LogCenter;

public class LogCenterLogger : ILogger
{
    private readonly HttpClient _client;
    private readonly string _url;
    private readonly string _table;
    private readonly Guid _correlation;

    private LogLevel _level;
    private string _message;
    private dynamic _data;

    private LogQueue _queue = LogQueue.Instance;

    public LogCenterLogger(LogCenterOptions options)
    {
        _url = options.url;
        _table = options.table;
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_url);

    }

    /// <summary>
    /// Start a thread to send the log to LogCenter
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public void Log(LogLevel level, string message, dynamic data = null)
    {
        Task.WaitAll(Task.Run(() =>Enqueue(level, message, data))); 
    }

    
    private async Task Enqueue(LogLevel level, string message, dynamic data)
    {
        _queue.Enqueue(_client, _table, level, message, data);
    }

    /// <summary>
    /// Send the log to LogCenter, and wait for a response. Use with await
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task LogAsync(LogLevel level, string message, dynamic data = null)
    {
        await EnqueueAsync(level, message, data); 
    }

    private async Task EnqueueAsync(LogLevel level, string message, dynamic data)
    {
        await _queue.EnqueueAsync(_client, _table, level, message, data);
    }
}
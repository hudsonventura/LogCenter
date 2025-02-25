
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

    
    public void Log(string message, LogLevel level = LogLevel.Information)
    {
        Task.WaitAll(Task.Run(() =>LogAsync(LogLevel.Information, message, null))); 
    }

    public async Task LogAsync(string message, LogLevel level = LogLevel.Information)
    {
        Task.WaitAll(Task.Run(() =>LogAsync(LogLevel.Information, message, null))); 
    }

    
    public void Log(dynamic data, LogLevel level = LogLevel.Information)
    {
        Task.WaitAll(Task.Run(() =>LogAsync(LogLevel.Information, null, data))); 
    }

    private async Task LogAsync(LogLevel level, string message, dynamic data)
    {
        _queue.Enqueue(_client, _table, level, message, data);
    }

    public  async Task LogAsync(int teste)
    {
        LogLevel level = LogLevel.Information;
        string message = "Hello World";
        dynamic data = new {};
        await _queue.EnqueueAsync(_client, _table, level, message, data);
        Console.WriteLine("Enviado!");
    }
}


namespace nuget;

public class LogCenterLogger : nuget.ILogger
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
        _client.DefaultRequestHeaders.Add("Timezone", options.timezone);
        _client.DefaultRequestHeaders.Add("Correlation", options.correlation.ToString());
    }

    public void LogInformation<T>(string message)
    {
        Task.WaitAll(Task.Run(() =>LogAsync(LogLevel.Information, message, null))); 
    }
    
    public void LogInformation(string message)
    {
        Task.WaitAll(Task.Run(() =>LogAsync(LogLevel.Information, message, null))); 
    }

    public void LogInformation<T>(dynamic data)
    {
        Task.WaitAll(Task.Run(() =>LogAsync(LogLevel.Information, null, data))); 
    }
    
    public void LogInformation(dynamic data)
    {
        Task.WaitAll(Task.Run(() =>LogAsync(LogLevel.Information, null, data))); 
    }

    public async Task LogAsync(LogLevel level, string message, dynamic data)
    {
        _queue.Enqueue(_client, _table, level, message, data);
    }


}
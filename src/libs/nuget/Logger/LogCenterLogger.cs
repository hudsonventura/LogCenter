
namespace LogCenter;

public class LogCenterLogger : ILogger
{
    private HttpClient _client;
    private string _url;
    private string _table;
    private string _traceId;

    private LogLevel _level;
    private string _message;
    private dynamic _data;

    private LogQueue _queue = LogQueue.Instance;

    /// <summary>
    /// LogCenterLogger constructor. Inicialize with a random traceId
    /// </summary>
    /// <param name="options"></param>
    public LogCenterLogger(LogCenterOptions options)
    {
        fullContructor(options, Guid.NewGuid().ToString());
    }

    /// <summary>
    /// LogCenterLogger constructor. Initialize with a Guid as traceId
    /// </summary>
    /// <param name="options"></param>
    public LogCenterLogger(LogCenterOptions options, Guid traceId)
    {
        fullContructor(options, traceId.ToString());
    }

    /// <summary>
    /// LogCenterLogger constructor. Initialize with a string as traceId
    /// </summary>
    /// <param name="options"></param>
    public LogCenterLogger(LogCenterOptions options, string traceId)
    {
        fullContructor(options, traceId);
    }

    /// <summary>
    /// The real constructor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="traceId"></param>
    private void fullContructor(LogCenterOptions options, string traceId){
        _url = options.url;
        _table = options.table;
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_url);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.token}");
        _traceId = traceId.ToString();
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
        _queue.Enqueue(_client, _table, level, _traceId, message, data);
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
        await _queue.EnqueueAsync(_client, _table, level, _traceId, message, data);
    }
}
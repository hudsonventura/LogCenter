
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace LogCenter;

public class LogCenterLogger : ILogger
{
    private HttpClient _client;
    private string _table;
    private string _GlobaltraceId;
    private bool _ConsoleLog;
    private bool _ConsoleEntryObject;


    /// <summary>
    /// LogCenterLogger constructor. Inicialize with a random traceId. A Guid is generated as traceId
    /// </summary>
    /// <param name="options"></param>
    public LogCenterLogger(LogCenterOptions options)
    {
        fullContructor(options, Guid.NewGuid().ToString());
    }

    /// <summary>
    /// LogCenterLogger constructor. Initialize with your Guid as traceId
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
        _validations(options);
        _table = options.table;
        _client = new HttpClient();
        _client.BaseAddress = new Uri(options.url);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.token}");
        _GlobaltraceId = traceId.ToString();
        _ConsoleLog = options.consoleLog;
        _ConsoleEntryObject = options.consoleLogEntireObject;
    }

    private void _validations(LogCenterOptions options){
        if (string.IsNullOrEmpty(options.url)) throw new Exception("LogCenterOptions URL is required");
        if (string.IsNullOrEmpty(options.table)) throw new Exception("LogCenterOptions TABLE is required");
        if (string.IsNullOrEmpty(options.token)) throw new Exception("LogCenterOptions TOKEN is required. Generate it on web interface");
    }




    /// <summary>
    /// Start a thread to send the log to LogCenter
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public void Log(LogLevel level, string message, dynamic data = null)
    {
        var timestamp = DateTime.UtcNow;
        ConsoleLog(timestamp, level, message, data);
        Task.Run(() =>ProcessAsync(timestamp, level, message, data, null)); 
    }

    /// <summary>
    /// Start a thread to send the log to LogCenter
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public void Log(LogLevel level, string message, Guid traceId, dynamic data = null)
    {
        var timestamp = DateTime.UtcNow;
        ConsoleLog(timestamp, level, message, data);
        Task.Run(() =>ProcessAsync(timestamp, level, message, data, traceId.ToString())); 
    }

    /// <summary>
    /// Start a thread to send the log to LogCenter
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public void Log(LogLevel level, string message, string traceId, dynamic data = null)
    {
        var timestamp = DateTime.UtcNow;
        ConsoleLog(timestamp, level, message, data);
        Task.Run(() =>ProcessAsync(timestamp, level, message, data, traceId)); 
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
        var timestamp = DateTime.UtcNow;
        ConsoleLog(timestamp, level, message, data);
        await ProcessAsync(timestamp, level, message, data, null); 
    }

    /// <summary>
    /// Send the log to LogCenter, and wait for a response. Use with await
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task LogAsync(LogLevel level, string message, Guid traceId, dynamic data = null)
    {
        var timestamp = DateTime.UtcNow;
        ConsoleLog(timestamp, level, message, data);
        await ProcessAsync(timestamp, level, message, data, traceId.ToString()); 
    }

    /// <summary>
    /// Send the log to LogCenter, and wait for a response. Use with await
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task LogAsync(LogLevel level, string message, string traceId, dynamic data = null)
    {
        var timestamp = DateTime.UtcNow;
        ConsoleLog(timestamp, level, message, data);
        await ProcessAsync(timestamp, level, message, data, traceId); 
    }
    


    private void ConsoleLog(DateTime timestamp, LogLevel level, string message, dynamic data = null)
    {
        if(!_ConsoleLog){
            return;
        }

        Type type = data?.GetType();

        ConsoleColor console_color_default = Console.ForegroundColor;
        ConsoleColor console_color = SwitchColor(level, console_color_default);

        Console.ForegroundColor = console_color;
        if(type == null || !_ConsoleEntryObject){
            Console.WriteLine($"{timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")} [{level.ToString().ToUpper()}] {message}");
        }else{
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"{timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")} [{level.ToString().ToUpper()}] {message}{Environment.NewLine}{json}{Environment.NewLine}");
        }
        Console.ForegroundColor = console_color_default; //return to default console color
    }

    private ConsoleColor SwitchColor(LogLevel level, ConsoleColor default_color){
        switch (level){
            case LogLevel.Debug:
                return ConsoleColor.DarkBlue;
            case LogLevel.Information:
                return ConsoleColor.Blue;
            case LogLevel.Warning:
                return ConsoleColor.Yellow;
            case LogLevel.Error:
                return ConsoleColor.Red;
            case LogLevel.Success:
                return ConsoleColor.Green;
            case LogLevel.Critical:
                return ConsoleColor.DarkRed;
            case LogLevel.Fatal:
                return ConsoleColor.DarkRed;
            case LogLevel.Trace:
                return ConsoleColor.DarkCyan;
        }

        return default_color;
    }



    private async Task ProcessAsync(DateTime timestamp, LogLevel level, string message, dynamic data, string traceId)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _table);
            request.Headers.Add("Level", level.ToString());


            //Add traceId from aspnet execution
            if(Activity.Current?.Id != null){
                //request.Headers.Add("TraceID", Activity.Current?.Id);
                traceId = Activity.Current?.Id;
            }

            if(traceId == null){
                traceId = _GlobaltraceId;
            }

            request.Headers.Add("TraceID", traceId);

            request.Headers.Add("Timestamp", timestamp.ToString("o"));
            request.Headers.Add("Message", message);
            

            if(data is not null){
                string json = JsonSerializer.Serialize(data);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                request.Content = content;
            }
        
            var response = await _client.SendAsync(request);
            if(!response.IsSuccessStatusCode){
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"It was failled to send log to LogCenter: {responseBody}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"It was failled to send log to LogCenter: {ex.Message}");
        }

    }

    
}
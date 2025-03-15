using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace LogCenter;

public sealed class LogQueue
{
    private static List<LogItem> queue = new List<LogItem>();

    private static LogQueue instance = new LogQueue();

    public static LogQueue Instance => instance;
    private LogQueue() { }
/*  */
    private bool IsWorking = false;


    public async Task Enqueue(HttpClient client, string table, LogLevel level, string traceId, string message, dynamic data)
    {
        queue.Add(new LogItem(){
            client = client,
            table = table,
            level = level,
            TraceID = traceId,
            message = message,
            data = data
        });
        if(!IsWorking){
            IsWorking = true;
            ProcessQueueAsync();
        }
    }

    public async Task EnqueueAsync(HttpClient client, string table, LogLevel level, string traceId, string message, dynamic data)
    {
        queue.Add(new LogItem(){
            client = client,
            table = table,
            level = level,
            TraceID = traceId,
            message = message,
            data = data
        });
        newTry:
        if(!IsWorking){
            IsWorking = true;
            await ProcessQueueAsync();
        }else{
            await Task.Delay(100);
            goto newTry;
        }
    }


    private async Task ProcessQueueAsync()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            var item = queue[i];
            if(item.isProcessed == true){
                continue;
            }
            item.isProcessed = true;
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, item.table);
                request.Headers.Add("Level", item.level.ToString());
                if(Activity.Current?.Id != null){
                    request.Headers.Add("TraceID", Activity.Current?.Id);
                }

                if(item.TraceID is not null){
                    request.Headers.Add("TraceID", item.TraceID);

                }

                request.Headers.Add("Message", item.message);
                

                if(item.data is not null){
                    string json = JsonSerializer.Serialize(item.data);
                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    request.Content = content;
                }
            
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} [{item.level.ToString().ToUpper()}] {item.message}");
                var response = await item.client.SendAsync(request);
                if(!response.IsSuccessStatusCode){
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"It was falled to send log to LogCenter: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"It was falled to send log to LogCenter: {ex.Message}");
            }
        }
        IsWorking = false;
    }
}


class LogItem{

    public bool isProcessed = false;
    public HttpClient client;

    public string table;
    public LogLevel level;
    public string message;
    public dynamic data;

    public string TraceID = null;
}
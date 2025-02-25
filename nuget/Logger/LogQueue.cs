using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using Serilog;

namespace LogCenter;

public sealed class LogQueue
{
    private static List<LogItem> queue = new List<LogItem>();

    private static LogQueue instance = new LogQueue();

    public static LogQueue Instance => instance;
    private LogQueue() { }
/*  */
    private bool IsWorking = false;


    public async Task Enqueue(HttpClient client, string table, LogLevel level, string message, dynamic data)
    {
        queue.Add(new LogItem(){
            client = client,
            table = table,
            level = level,
            message = message,
            data = data
        });
        if(!IsWorking){
            IsWorking = true;
            ProcessQueueAsync();
        }
    }

    public async Task EnqueueAsync(HttpClient client, string table, LogLevel level, string message, dynamic data)
    {
        queue.Add(new LogItem(){
            client = client,
            table = table,
            level = level,
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
                // Aqui você pode personalizar o envio dos logs para onde quiser
                Console.WriteLine($"{DateTime.Now} [{item.level.ToString()}] {item.message}");

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, item.table);
                //request.Headers.Add("Level", logEvent.Level.ToString());
                if(Activity.Current?.Id != null){
                    request.Headers.Add("Correlation", Activity.Current?.Id);
                }
                

                string json = JsonSerializer.Serialize(item.message);
                if(item.data is not null){
                    json = JsonSerializer.Serialize(item.data);
                }
            
                
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                request.Content = content;

                var response = await item.client.SendAsync(request);
                //string responseBody = await response.Content.ReadAsStringAsync(); 
                //var objSucesso = JsonSerializer.Deserialize<dynamic>(responseBody);

                
                Console.Write("");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar requisição: {ex.Message}");
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
}
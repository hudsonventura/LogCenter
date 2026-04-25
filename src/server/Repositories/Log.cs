using System.Net.Http.Json;
using server.Domain;

namespace server.Repositories;

public class Log
{

    internal static void RegisterLog(LogLevel level, string execution_id, string log)
    {
        HttpClient _client = new HttpClient();
        _client = new HttpClient();
        _client.BaseAddress = new Uri("http://localhost:9200/");

        Console.WriteLine($"{level.ToString()}: {execution_id} -> {log}");

        var payload = new RequestRecord
        {
            Message = execution_id,
            Category = LogCategory.Log,
            Timestamp = DateTime.UtcNow,
            Level = level,
            Content = log
        };
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,"/LogCenter_JobExecution")
        {
            Content = JsonContent.Create(payload)
        };

        try
        {
            HttpResponseMessage response = _client.SendAsync(request).Result;
        }
        catch (System.Exception error)
        {
            string msg = error.Message;
            if(error.InnerException != null){
                msg += error.InnerException.Message;
            }
            Console.WriteLine(msg);
        }
		
    }
}

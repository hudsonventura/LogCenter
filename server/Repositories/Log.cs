
using System.Text;
using server.Domain;

namespace server.Repositories;

public class Log
{

    internal static void RegisterLog(Level level, string execution_id, string log)
    {
        HttpClient _client = new HttpClient();
        _client = new HttpClient();
        _client.BaseAddress = new Uri("http://localhost:9200/");

        Console.WriteLine($"{level.ToString()}: {execution_id} -> {log}");


        HttpContent content = new StringContent($"\"{log}\"", Encoding.UTF8, "application/json");
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,"/LogCenter_JobExecution")
        {
            Content = content
        };
        request.Headers.Add("description", execution_id);
        request.Headers.Add("level", level.ToString());

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

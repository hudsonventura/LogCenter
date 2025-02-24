using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;

namespace LogCenter.RequestInterceptor;

public class Response
{
    public string Type { get; private set; } = "Response";
    public string SentToAddress { get; private set; }
    public int StatusCode { get; private set; }
    public string ReasonPhrase { get; private set; }
    public Dictionary<string, string> Headers { get; private set; }
    public string Body { get; private set; }

    public Exception Exception  { get; private set; }

    internal static async Task<Response> Convert(Microsoft.AspNetCore.Http.HttpContext context, Exception error){

        HttpResponse response = context.Response;

        return new Response(){
            SentToAddress = context.Connection.RemoteIpAddress.ToString(),
            StatusCode = response.StatusCode,
            ReasonPhrase = ((HttpStatusCode)response.StatusCode).ToString(),
            Headers = response.Headers.ToDictionary(q => q.Key, q => q.Value.ToString()),
            Exception = error
        };
    }

    internal static async Task<Response> Convert(Microsoft.AspNetCore.Http.HttpContext context, string body)
    {
        HttpResponse response = context.Response;

        string bodyLines = (body.Length > 0) ? body : "null";
        if (response.Headers.ContainsKey("Content-Type") && response.Headers["Content-Type"].ToString().Contains("json"))
        {

            // Serializa o objeto de volta para uma string JSON formatada
            dynamic objetoDynamic = JsonSerializer.Deserialize<dynamic>(body);
            //bodyLines = JsonConvert.SerializeObject(objetoDynamic, settings);
            bodyLines = JsonSerializer.Serialize(objetoDynamic, new JsonSerializerOptions { WriteIndented = true });

        }
        bodyLines = string.Join(Environment.NewLine, bodyLines.Split('\n').Select(line => line));


        return new Response(){
            SentToAddress = context.Connection.RemoteIpAddress.ToString(),
            StatusCode = response.StatusCode,
            ReasonPhrase = ((HttpStatusCode)response.StatusCode).ToString(),
            Headers = response.Headers.ToDictionary(q => q.Key, q => q.Value.ToString()),
            Body = bodyLines,
        };
    }


    public override string ToString()
    {
        string headers_string = string.Join("\n", Headers.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        string body = (Body is not null) ? $"\n\n{Body}" : "";
        string exception = (Exception is not null) ? $"\n\n{Exception}" : "";


        return $"{StatusCode} {ReasonPhrase}\nSent to Address: {SentToAddress}\n{headers_string}{body}{exception}";
    }
}
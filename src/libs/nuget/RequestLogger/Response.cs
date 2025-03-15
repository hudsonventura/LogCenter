using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;

namespace LogCenter.RequestInterceptor;

public class Response
{
    [System.Text.Json.Serialization.JsonPropertyOrder(0)]
    public string Type { get; private set; } = "Response";

    [System.Text.Json.Serialization.JsonPropertyOrder(1)]
    public string SentToAddress { get; private set; }

    [System.Text.Json.Serialization.JsonPropertyOrder(2)]
    public int StatusCode { get; private set; }

    [System.Text.Json.Serialization.JsonPropertyOrder(3)]
    public string ReasonPhrase { get; private set; }

    [System.Text.Json.Serialization.JsonPropertyOrder(4)]
    public Dictionary<string, string> Headers { get; private set; }

    [System.Text.Json.Serialization.JsonPropertyOrder(5)]
    public string Body { get; private set; }

    [System.Text.Json.Serialization.JsonPropertyOrder(6)]
    public Exception Exception { get; private set; }

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


        return $"Response\n{StatusCode} {ReasonPhrase}\nSent to Address: {SentToAddress}\n{headers_string}{body}{exception}";
    }
}
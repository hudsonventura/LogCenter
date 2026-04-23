using System.Text;
using Microsoft.AspNetCore.Http;


namespace LogCenter.RequestInterceptor;

public sealed class Request
{
    [System.Text.Json.Serialization.JsonPropertyOrder(0)]
    public string Type { get; private set; } = "Request";

    [System.Text.Json.Serialization.JsonPropertyOrder(1)]
    public string ReceivedFromAddress { get; private set; }

    [System.Text.Json.Serialization.JsonPropertyOrder(2)]
    public string Method { get; private set; }
    public Dictionary<string, string> Headers { get; private set; }
    public string Host { get; private set; }
    public string Path { get; private set; }
    public Dictionary<string, string> Query { get; private set; }
    public List<RequestParameter> Parameters { get; private set; }
    public string CompleteURL { get; private set; }
    public string Body { get; private set; }

    internal static async Task<Request> Convert(Microsoft.AspNetCore.Http.HttpContext context)
    {
        HttpRequest request = context.Request;
        var completeURL = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}";
        var parameters = request.Query
            .SelectMany(
                static query => query.Value,
                static (query, value) => new RequestParameter(query.Key, value ?? string.Empty))
            .ToList();


        return new Request(){
            ReceivedFromAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            Method = request.Method,
            Host = request.Host.ToString(),
            Path = $"{request.PathBase}{request.Path}",
            CompleteURL = completeURL,
            Query = request.Query.ToDictionary(q => q.Key, q => q.Value.ToString()),
            Parameters = parameters,
            Headers = request.Headers.ToDictionary(q => q.Key, q => q.Value.ToString()),
            Body = await ReadBody(request),
        };

        
    }

    private async static Task<string> ReadBody(HttpRequest request)
    {
        // Configure o corpo da requisição para permitir a leitura posterior
        request.EnableBuffering();

        // Lê o corpo da requisição como uma string sem consumi-lo
        using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
        {
            string body = await reader.ReadToEndAsync();

            // Volta ao início do fluxo para que o corpo possa ser lido novamente posteriormente
            request.Body.Seek(0, SeekOrigin.Begin);

            return (body.Length > 0) ? body : "null";
        }
    }



    public override string ToString()
    {
        string headers_string = string.Join("\n", Headers.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        string parameters = Parameters.Count > 0
            ? "\nParams: " + string.Join(", ", Parameters.Select(static parameter => $"{parameter.Name}={parameter.Value}"))
            : string.Empty;


        return $"Request\n{Method} {CompleteURL}{parameters}\n{headers_string}\n\n{Body}";
    }
}

public sealed record RequestParameter(string Name, string Value);

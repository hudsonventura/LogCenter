using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace nuget;

public class LogCenterSink : ILogEventSink, Microsoft.Extensions.Logging.ILogger
{
    private readonly HttpClient _client;
    private readonly string _url;
    private readonly string _table;
    private readonly string _timezone;

    public LogCenterSink(LogCenterSinkOptions options)
    {
        _url = options.url;
        _table = options.table;
        _client = new HttpClient();
        _timezone = options.timezone;
    }

    
    public void Emit(LogEvent logEvent)
    {
        Task.Run(() =>EmitAsync(logEvent));
    }

    public async Task EmitAsync(LogEvent logEvent){
        // Aqui você pode personalizar o envio dos logs para onde quiser
        var message = logEvent.RenderMessage();
        Console.WriteLine($"[LogCenter] {logEvent.Timestamp}: {message}");

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{_url}/{_table}");
        request.Headers.Add("Timezone", _timezone);
        //request.Headers.Add("Level", logEvent.Level.ToString());

        string json = JsonSerializer.Serialize(logEvent);

        if (logEvent.Properties.TryGetValue("test", out var testProperty))
        {
            if (testProperty is StructureValue structure)
            {
                var dictionary = structure.Properties.ToDictionary(p => p.Name, p => ((ScalarValue)p.Value).Value);
                json = JsonSerializer.Serialize(dictionary, new JsonSerializerOptions { WriteIndented = true });
            }
            else if (testProperty is SequenceValue sequence)
            {
                var list = sequence.Elements.Select(item => item.ToString()).ToList();
                json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            }
            else
            {
                json = testProperty.ToString();
            }


            Console.WriteLine($"[LogCenter] Lista de objetos recebida:\n{json}");
        }

        
        
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        request.Content = content;

        var response = await _client.SendAsync(request);
        string responseBody = await response.Content.ReadAsStringAsync(); 
        var objSucesso = JsonSerializer.Deserialize<dynamic>(responseBody);
        Console.WriteLine(objSucesso);

        // Exemplo: Se quiser enviar para uma API, pode fazer algo como:
        // HttpClient.PostAsync("http://seu-log-center.com/api/logs", new StringContent(json));
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        // Mensagem de log formatada
        var logMessage = formatter(state, exception);

        // Dicionário para armazenar propriedades do state
        var stateData = new Dictionary<string, object>();

        if (state is IEnumerable<KeyValuePair<string, object>> keyValuePairs)
        {
            foreach (var kvp in keyValuePairs)
            {
                stateData[kvp.Key] = kvp.Value;
            }
        }

        try
        {
            foreach (var item in stateData)
            {
                // Serializar objetos encontrados
                string jsonState = stateData.Count > 0
                    ? JsonSerializer.Serialize(stateData, new JsonSerializerOptions { WriteIndented = true })
                    : "Nenhum objeto encontrado no state.";

                Console.WriteLine($"[{logLevel}] CATEGORIA: {logMessage}");
                Console.WriteLine($"[State JSON]: {jsonState}");
            }
            

            if (exception != null)
            {
                Console.WriteLine($"[Exception]: {exception}");
            }
        }
        catch (System.Exception)
        {
            
        }
        
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return new DummyDisposable();
    }

    private class DummyDisposable : IDisposable
    {
        public void Dispose()
        {
            // N o faz nada
        }

    }

    public void Write(LogEvent logEvent)
    {
        Task.Run(() =>EmitAsync(logEvent));
    }
}
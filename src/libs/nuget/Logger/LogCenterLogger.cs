using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LogCenter;

/// <summary>
/// <see cref="ILogger.Log"/> da BCL é sempre síncrono: o <see cref="LogCenterLogPayload"/> é montado aqui, na mesma chamada.
/// O I/O HTTP fica em <see cref="SendAsync"/> (assíncrono), agendado após <see cref="Log"/> retornar, salvo se
/// <see cref="LogCenterOptions.SendSynchronously"/> estiver ativo.
/// </summary>
internal sealed class LogCenterLogger : ILogger
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly string _categoryName;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LogCenterOptions _options;

    public LogCenterLogger(
        string categoryName,
        IHttpClientFactory httpClientFactory,
        LogCenterOptions options)
    {
        _categoryName = categoryName;
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) =>
        _options.Enabled && logLevel != LogLevel.None && logLevel >= _options.MinimumLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        //if (!IsEnabled(logLevel))
        //    return;

        var message = formatter(state, exception);
        var structured = LogCenterStructuredState.TryBuildStructuredProperties(
            state,
            JsonOptions,
            _options.StripDestructuringAtPrefix);
        var payload = new LogCenterLogPayload
        {
            Timestamp = DateTimeOffset.UtcNow,
            Level = logLevel.ToString(),
            Category = _categoryName,
            Message = message,
            Exception = exception?.ToString(),
            ApplicationName = _options.ApplicationName,
            StructuredProperties = structured
        };

        string json = JsonSerializer.Serialize(structured);
        //Debug.WriteLine("[LogFull] Template", state.);
        Debug.WriteLine("[LogFull] Message", message);
        Debug.WriteLine("[LogFull] Object", json);

        // Daqui em diante o payload já existe na thread atual (síncrono).
        // if (_options.SendSynchronously)
        //     SendAsync(payload).GetAwaiter().GetResult();
        // else
        //     _ = SendAsync(payload);
    }

    private async Task SendAsync(LogCenterLogPayload payload)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(LogCenterOptions.HttpClientName);
            var requestUri = ResolveRequestUri(client);
            if (requestUri is null)
            {
                Debug.WriteLine(
                    "[LogFull] Envio ignorado: defina BaseAddress no HttpClient nomeado ou use LogEndpoint como URL absoluta.");
                return;
            }

            using var response = await client.PostAsJsonAsync(requestUri, payload, JsonOptions).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LogFull] Falha ao enviar log: {ex.Message}");
        }
    }

    private string? ResolveRequestUri(HttpClient client)
    {
        var ep = _options.LogEndpoint.Trim();
        if (Uri.TryCreate(ep, UriKind.Absolute, out _))
            return ep;

        if (client.BaseAddress is null)
            return null;

        return ep.StartsWith('/') ? ep : "/" + ep;
    }

    private sealed class NullScope : IDisposable
    {
        internal static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

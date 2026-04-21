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
    private readonly HttpClient _httpClient;
    private readonly LogCenterOptions _options;

    public LogCenterLogger(
        string categoryName,
        HttpClient httpClient,
        LogCenterOptions options)
    {
        _categoryName = categoryName;
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
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

        string payloadJson = JsonSerializer.Serialize(structured, JsonOptions);
        //Console.WriteLine($"[LogCenter] Payload para enviar:\n{payloadJson}\n");


        ShowConsole(logLevel, message);

            

        // Daqui em diante o payload já existe na thread atual (síncrono).
        if (_options.SendSynchronously)
            SendAsync(payload).GetAwaiter().GetResult();
        else
            _ = SendAsync(payload);
    }

    private async Task SendAsync(LogCenterLogPayload payload)
    {
        // TODO: Descomente quando o objeto chegar corretamente
        /*
        try
        {
            var requestUri = ResolveRequestUri();
            if (requestUri is null)
            {
                Debug.WriteLine(
                    "[LogFull] Envio ignorado: defina BaseAddress no HttpClient ou use LogEndpoint como URL absoluta.");
                return;
            }

            using var response = await _httpClient.PostAsJsonAsync(requestUri, payload, JsonOptions).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LogFull] Falha ao enviar log: {ex.Message}");
        }
        */
    }

    private string? ResolveRequestUri()
    {
        var ep = _options.LogEndpoint.Trim();
        if (Uri.TryCreate(ep, UriKind.Absolute, out _))
            return ep;

        if (_httpClient.BaseAddress is null)
            return null;

        return ep.StartsWith('/') ? ep : "/" + ep;
    }

    private sealed class NullScope : IDisposable
    {
        internal static readonly NullScope Instance = new();
        public void Dispose() { }
    }


    private void ShowConsole(LogLevel level, string message){
        var default_foreground_color = Console.ForegroundColor;
        var default_background_color = Console.BackgroundColor;

        Console.ForegroundColor = SwitchColor(level, default_foreground_color);
        Console.BackgroundColor = ConsoleColor.Black;

        var formattedLevel = FormatLevel(level.ToString());
        Console.Write(formattedLevel + " ");

        Console.ForegroundColor = default_foreground_color;
        Console.BackgroundColor = default_background_color;

        Console.WriteLine(message);
    }

    private string FormatLevel(string level)
    {
        // Total de 15 caracteres: [ + espaço + 11 caracteres centralizados + espaço + ]
        const int contentWidth = 11;
        
        string padded = level.Length < contentWidth 
            ? level.PadLeft((contentWidth + level.Length) / 2).PadRight(contentWidth)
            : level.Substring(0, contentWidth);
        
        return $"[ {padded} ]";
    }

    private ConsoleColor SwitchColor(LogLevel level, ConsoleColor default_color){
        switch (level){
            case LogLevel.Debug:
                return ConsoleColor.Cyan;
            case LogLevel.Information:
                return ConsoleColor.Blue;
            case LogLevel.Warning:
                return ConsoleColor.Yellow;
            case LogLevel.Error:
                return ConsoleColor.Red;
            case LogLevel.Critical:
                return ConsoleColor.Red;
            case LogLevel.Trace:
                return ConsoleColor.DarkCyan;
            default: return default_color;
        }

        return default_color;
    }
}

using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LogCenter;

/// <summary>
/// <see cref="ILogger.Log"/> da BCL é sempre síncrono: o <see cref="LogCenterLogPayload"/> é montado aqui, na mesma chamada.
/// O I/O HTTP fica em <see cref="SendAsync"/> (assíncrono), agendado após <see cref="Log"/> retornar, salvo se
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
        var messageTemplate = TryExtractMessageTemplate(state);

        var structured = LogCenterStructuredState.TryBuildStructuredProperties(
            state,
            JsonOptions,
            _options.StripDestructuringAtPrefix);
        var payload = new LogCenterLogPayload
        {
            Table = _options.Table,
            Timestamp = DateTimeOffset.UtcNow,
            Level = logLevel.ToString(),
            TraceId = TryExtractTraceId(structured) ?? Activity.Current?.Id,
            Category = _categoryName,
            Message = message,
            ApplicationName = _options.ApplicationName,
            Exception = exception?.ToString(),
            StructuredProperties = structured
        };

        _ = SendAsync(messageTemplate, DateTime.UtcNow, logLevel, payload);

        ShowConsole(logLevel, message);
    }

    private async Task SendAsync(string message, DateTime timestamp, LogLevel logLevel, LogCenterLogPayload payload)
    {
        try
        {
            if (_options.Url is null)
            {
                return;
            }
            HttpRequestMessage request = new(HttpMethod.Post, $"{_options.Url}/{_options.Table}");
            request.Headers.Add("Message", ToAscii(message));
            request.Headers.Add("Timestamp", timestamp.ToString("o"));
            request.Headers.Add("Level", logLevel.ToString());

            if (!string.IsNullOrWhiteSpace(payload.TraceId))
                request.Headers.Add("TraceId", payload.TraceId);

            if (payload.StructuredProperties is not null)
            {
                var content = BuildLegacyContent(payload);
                if (content is not null)
                    request.Content = JsonContent.Create(content, mediaType: new MediaTypeHeaderValue("application/json"));
            }

            using var response =  await _httpClient.SendAsync(request).ConfigureAwait(false);

            //using var response = await _httpClient.PostAsJsonAsync(requestUri, payload, JsonOptions).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return;

            if (!ShouldFallbackToLegacyEndpoint(response))
            {
                response.EnsureSuccessStatusCode();
                return;
            }

            using var legacyRequest = BuildLegacyRequest(payload);
            using var legacyResponse = await _httpClient.SendAsync(legacyRequest).ConfigureAwait(false);
            legacyResponse.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LogCenter] Fail sending log: {ex.Message}");
        }
    }

    private HttpRequestMessage BuildLegacyRequest(LogCenterLogPayload payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, ResolveLegacyRequestUri());
        request.Headers.Add("Level", payload.Level);
        request.Headers.Add("Timestamp", payload.Timestamp.ToString("o"));
        request.Headers.Add("Message", payload.Message);

        if (!string.IsNullOrWhiteSpace(payload.TraceId))
            request.Headers.Add("TraceId", payload.TraceId);

        var content = BuildLegacyContent(payload);
        if (content is not null)
            request.Content = JsonContent.Create(content, mediaType: new MediaTypeHeaderValue("application/json"));

        return request;
    }

    private static Dictionary<string, object?>? BuildLegacyContent(LogCenterLogPayload payload)
    {
        Dictionary<string, object?>? content = null;

        if (!string.IsNullOrWhiteSpace(payload.Category))
        {
            content ??= new Dictionary<string, object?>(StringComparer.Ordinal);
            content["Category"] = payload.Category;
        }

        if (!string.IsNullOrWhiteSpace(payload.ApplicationName))
        {
            content ??= new Dictionary<string, object?>(StringComparer.Ordinal);
            content["ApplicationName"] = payload.ApplicationName;
        }

        if (!string.IsNullOrWhiteSpace(payload.Exception))
        {
            content ??= new Dictionary<string, object?>(StringComparer.Ordinal);
            content["Exception"] = payload.Exception;
        }

        if (payload.StructuredProperties is not null)
        {
            content ??= new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var (key, value) in payload.StructuredProperties)
                content[key] = value.Clone();
        }

        return content;
    }

    private string ResolveLegacyRequestUri()
    {
        var table = payloadPathSegment(_options.Table);
        return "/" + table;

        static string payloadPathSegment(string value) =>
            value.Trim().Trim('/').Replace(" ", "_", StringComparison.Ordinal).ToLowerInvariant();
    }

    private static bool ShouldFallbackToLegacyEndpoint(HttpResponseMessage response) =>
        response.StatusCode is System.Net.HttpStatusCode.NotFound
            or System.Net.HttpStatusCode.MethodNotAllowed
            or System.Net.HttpStatusCode.NotImplemented;

    private static string? TryExtractTraceId(Dictionary<string, JsonElement>? structuredProperties)
    {
        if (structuredProperties is null)
            return null;

        foreach (var key in new[] { "TraceId", "traceId" })
        {
            if (!structuredProperties.TryGetValue(key, out var value))
                continue;

            if (value.ValueKind == JsonValueKind.String)
                return value.GetString();
        }

        return null;
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
    }


    private static string? TryExtractMessageTemplate<TState>(TState state)
    {
        if (state is not IEnumerable<KeyValuePair<string, object?>> pairs)
            return null;

        foreach (var kv in pairs)
        {
            if (string.Equals(kv.Key, "{OriginalFormat}", StringComparison.Ordinal) ||
                string.Equals(kv.Key, "OriginalFormat", StringComparison.Ordinal))
            {
                return kv.Value?.ToString();
            }
        }

        return null;
    }

    public static string ToAscii(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder(input.Length);

        foreach (char c in input)
        {
            if (c <= 127) // ASCII vai de 0 a 127
                sb.Append(c);
        }

        return sb.ToString();
    }

}

using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LogCenter;

/// <summary>
/// <see cref="ILogger.Log"/> da BCL é sempre síncrono: o <see cref="LogCenterLogPayload"/> é montado aqui, na mesma chamada.
/// O I/O HTTP fica em <see cref="SendAsync"/> (assíncrono), agendado após <see cref="Log"/> retornar, salvo se
/// </summary>
internal sealed class LogCenterLogger : ILogger
{
    private const string TimestampPropertyName = "__LogCenterTimestamp";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly string _categoryName;
    private readonly HttpClient _httpClient;
    private readonly LogCenterOptions _options;
    private readonly LogCenterPendingSendTracker _pendingSendTracker;
    private readonly HashSet<int> _bannedEventIds;
    private readonly HashSet<string> _bannedEventNames;
    private readonly HashSet<string> _bannedMessages;

    public LogCenterLogger(
        string categoryName,
        HttpClient httpClient,
        LogCenterOptions options,
        LogCenterPendingSendTracker pendingSendTracker)
    {
        _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _pendingSendTracker = pendingSendTracker ?? throw new ArgumentNullException(nameof(pendingSendTracker));
        _bannedEventIds = new HashSet<int>(_options.BannedEventIds);
        _bannedEventNames = new HashSet<string>(_options.BannedEventNames, StringComparer.Ordinal);
        _bannedMessages = new HashSet<string>(_options.BannedMessages, StringComparer.Ordinal);
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) =>
        logLevel != LogLevel.None
        && logLevel >= _options.MinimumLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if(CheckBannedEventId(eventId))
            return;

        

        var now = TryExtractTimestamp(state) ?? DateTimeOffset.UtcNow;
        var message = formatter(state, exception);
        var messageTemplate = TryExtractMessageTemplate(state);

        if(CheckBannedMessage(messageTemplate ?? message ?? string.Empty))
            return;

        var structured = LogCenterStructuredState.TryBuildStructuredProperties(
            state,
            JsonOptions,
            _options.StripDestructuringAtPrefix);
        if (exception is not null)
        {
            structured ??= new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            structured["Exception"] = LogCenterStructuredState.SerializeValue(exception, JsonOptions);
        }

        var payload = new RequestRecord
        {
            Message = $"{eventId.Name} {messageTemplate}" ?? message?? string.Empty,
            Timestamp = now.UtcDateTime,
            Level = logLevel,
            TraceId = TryExtractTraceId(structured) ?? Activity.Current?.Id,
            Content = structured,
            //MessageTemplate = messageTemplate,
        };

        QueueSend(payload);

        ShowConsole(logLevel, message ?? string.Empty);
    }

    private bool CheckBannedEventId(EventId eventId)
    {
        return _bannedEventIds.Contains(eventId.Id)
            || (!string.IsNullOrWhiteSpace(eventId.Name) && _bannedEventNames.Contains(eventId.Name));
    }

    private bool CheckBannedMessage(string message)
    {
        return !string.IsNullOrWhiteSpace(message) && _bannedMessages.Contains(message);
    }

    private void QueueSend(RequestRecord payload)
    {
        if (!_pendingSendTracker.TryBegin())
        {
            SendAsync(payload).GetAwaiter().GetResult();
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await SendAsync(payload).ConfigureAwait(false);
            }
            finally
            {
                _pendingSendTracker.Complete();
            }
        });
    }

    private async Task SendAsync(RequestRecord payload)
    {
        try
        {
            if (_httpClient.BaseAddress is null)
            {
                return;
            }
            using var request = BuildRequestRecordRequest(payload);

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return;


        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LogCenter] Fail sending log: {ex.Message}");
        }
    }



    private HttpRequestMessage BuildRequestRecordRequest(RequestRecord payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, ResolveLegacyRequestUri())
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        return request;
    }

    private static Dictionary<string, object?>? BuildRequestContent(LogCenterLogPayload payload)
    {
        Dictionary<string, object?>? content = null;



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

    private static int ResolveLevel(LogCenterLogPayload payload)
    {
        if (Enum.TryParse<LogLevel>(payload.Level, ignoreCase: true, out var level))
        {
            return level switch
            {
                LogLevel.Trace => 0,
                LogLevel.Debug => 2,
                LogLevel.Information => 1,
                LogLevel.Warning => 3,
                LogLevel.Error => 4,
                LogLevel.Critical => 5,
                _ => 1
            };
        }

        return 1;
    }


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

    private static DateTimeOffset? TryExtractTimestamp<TState>(TState state)
    {
        if (state is not IEnumerable<KeyValuePair<string, object?>> pairs)
            return null;

        foreach (var kv in pairs)
        {
            if (!string.Equals(kv.Key, TimestampPropertyName, StringComparison.Ordinal))
                continue;

            return kv.Value switch
            {
                DateTimeOffset dto => dto,
                DateTime dt => new DateTimeOffset(dt),
                string s when DateTimeOffset.TryParse(s, out var parsed) => parsed,
                _ => null
            };
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
            return string.Empty;

        foreach (var kv in pairs)
        {
            if (string.Equals(kv.Key, "{OriginalFormat}", StringComparison.Ordinal) ||
                string.Equals(kv.Key, "OriginalFormat", StringComparison.Ordinal))
            {
                return kv.Value?.ToString();
            }
        }

        return string.Empty;
    }

}

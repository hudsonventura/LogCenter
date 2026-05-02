using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace LogCenter;

/// <summary>
/// Extensões opcionais para enviar propriedades estruturadas mesmo quando a mensagem não usa placeholders.
/// </summary>
public static class LogCenterLoggerPropertyExtensions
{
    /// <summary>
    /// Registra uma mensagem com propriedades estruturadas extras, sem depender de placeholders no template.
    /// </summary>
    public static void Log(
        this ILogger logger,
        LogLevel logLevel,
        string? message,
        params (string Key, object? Value)[] properties) =>
        Log(logger, logLevel, eventId: default, exception: null, message, properties);

    /// <summary>
    /// Registra uma mensagem com propriedades estruturadas extras, sem colidir com os overloads nativos do ILogger.
    /// </summary>
    public static void LogWithProperties(
        this ILogger logger,
        LogLevel logLevel,
        string? message,
        params (string Key, object? Value)[] properties) =>
        Log(logger, logLevel, eventId: default, exception: null, message, properties);

    /// <summary>
    /// Registra uma mensagem com propriedades estruturadas extras, <see cref="EventId"/> e exceção opcionais.
    /// </summary>
    public static void Log(
        this ILogger logger,
        LogLevel logLevel,
        EventId eventId,
        Exception? exception,
        string? message,
        params (string Key, object? Value)[] properties)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(properties);

        var state = LogCenterPropertyState.Create(message, properties);
        logger.Log(logLevel, eventId, state, exception, static (currentState, _) => currentState.ToString());
    }

    /// <summary>
    /// Registra uma mensagem com um único argumento. Se a mensagem não tiver placeholders,
    /// o argumento é enviado como propriedade estruturada usando o nome capturado da expressão.
    /// </summary>
    public static void LogInformation(
        this ILogger logger,
        string? message,
        object? value,
        [CallerArgumentExpression("value")] string? valueExpression = null)
    {
        if (ShouldUseMicrosoftTemplateLogging(message))
        {
            LoggerExtensions.LogInformation(logger, message, value);
            return;
        }

        Log(logger, LogLevel.Information, message, CreateProperty(value, valueExpression));
    }

    /// <summary>
    /// Registra uma mensagem com um único argumento. Se a mensagem não tiver placeholders,
    /// o argumento é enviado como propriedade estruturada usando o nome capturado da expressão.
    /// </summary>
    public static void LogWarning(
        this ILogger logger,
        string? message,
        object? value,
        [CallerArgumentExpression("value")] string? valueExpression = null)
    {
        if (ShouldUseMicrosoftTemplateLogging(message))
        {
            LoggerExtensions.LogWarning(logger, message, value);
            return;
        }

        Log(logger, LogLevel.Warning, message, CreateProperty(value, valueExpression));
    }

    /// <summary>
    /// Registra uma exceção com um único argumento extra. Se a mensagem não tiver placeholders,
    /// o argumento é enviado como propriedade estruturada usando o nome capturado da expressão.
    /// </summary>
    public static void LogWarning(
        this ILogger logger,
        string? message,
        object? value,
        Exception exception,
        [CallerArgumentExpression("value")] string? valueExpression = null)
    {
        if (ShouldUseMicrosoftTemplateLogging(message))
        {
            LoggerExtensions.LogWarning(logger, exception, message, value);
            return;
        }

        Log(
            logger,
            LogLevel.Warning,
            eventId: default,
            exception,
            message,
            CreateProperty(value, valueExpression));
    }

    /// <summary>
    /// Registra uma mensagem com um único argumento. Se a mensagem não tiver placeholders,
    /// o argumento é enviado como propriedade estruturada usando o nome capturado da expressão.
    /// </summary>
    public static void LogError(
        this ILogger logger,
        string? message,
        object? value,
        [CallerArgumentExpression("value")] string? valueExpression = null)
    {
        if (ShouldUseMicrosoftTemplateLogging(message))
        {
            LoggerExtensions.LogError(logger, message, value);
            return;
        }

        Log(logger, LogLevel.Error, message, CreateProperty(value, valueExpression));
    }

    /// <summary>
    /// Registra uma exceção com um único argumento extra. Se a mensagem não tiver placeholders,
    /// o argumento é enviado como propriedade estruturada usando o nome capturado da expressão.
    /// </summary>
    public static void LogError(
        this ILogger logger,
        string? message,
        object? value,
        Exception exception,
        [CallerArgumentExpression("value")] string? valueExpression = null)
    {
        if (ShouldUseMicrosoftTemplateLogging(message))
        {
            LoggerExtensions.LogError(logger, exception, message, value);
            return;
        }

        Log(
            logger,
            LogLevel.Error,
            eventId: default,
            exception,
            message,
            CreateProperty(value, valueExpression));
    }

    /// <summary>
    /// Registra uma exceção com propriedades estruturadas extras, sem colidir com os overloads nativos do ILogger.
    /// </summary>
    public static void LogWithProperties(
        this ILogger logger,
        LogLevel logLevel,
        Exception exception,
        string? message,
        params (string Key, object? Value)[] properties) =>
        Log(logger, logLevel, eventId: default, exception, message, properties);

    /// <summary>
    /// Registra uma mensagem com um único argumento. Se a mensagem não tiver placeholders,
    /// o argumento é enviado como propriedade estruturada usando o nome capturado da expressão.
    /// </summary>
    public static void LogCritical(
        this ILogger logger,
        string? message,
        object? value,
        [CallerArgumentExpression("value")] string? valueExpression = null)
    {
        if (ShouldUseMicrosoftTemplateLogging(message))
        {
            LoggerExtensions.LogCritical(logger, message, value);
            return;
        }

        Log(logger, LogLevel.Critical, message, CreateProperty(value, valueExpression));
    }

    /// <summary>
    /// Registra uma exceção com um único argumento extra. Se a mensagem não tiver placeholders,
    /// o argumento é enviado como propriedade estruturada usando o nome capturado da expressão.
    /// </summary>
    public static void LogCritical(
        this ILogger logger,
        string? message,
        object? value,
        Exception exception,
        [CallerArgumentExpression("value")] string? valueExpression = null)
    {
        if (ShouldUseMicrosoftTemplateLogging(message))
        {
            LoggerExtensions.LogCritical(logger, exception, message, value);
            return;
        }

        Log(
            logger,
            LogLevel.Critical,
            eventId: default,
            exception,
            message,
            CreateProperty(value, valueExpression));
    }

    /// <summary>
    /// Registra uma mensagem com um único argumento. Se a mensagem não tiver placeholders,
    /// o argumento é enviado como propriedade estruturada usando o nome capturado da expressão.
    /// </summary>
    public static void LogDebug(
        this ILogger logger,
        string? message,
        object? value,
        [CallerArgumentExpression("value")] string? valueExpression = null)
    {
        if (ShouldUseMicrosoftTemplateLogging(message))
        {
            LoggerExtensions.LogDebug(logger, message, value);
            return;
        }

        Log(logger, LogLevel.Debug, message, CreateProperty(value, valueExpression));
    }

    /// <summary>
    /// Registra uma exceção com um único argumento extra. Se a mensagem não tiver placeholders,
    /// o argumento é enviado como propriedade estruturada usando o nome capturado da expressão.
    /// </summary>
    public static void LogDebug(
        this ILogger logger,
        string? message,
        object? value,
        Exception exception,
        [CallerArgumentExpression("value")] string? valueExpression = null)
    {
        if (ShouldUseMicrosoftTemplateLogging(message))
        {
            LoggerExtensions.LogDebug(logger, exception, message, value);
            return;
        }

        Log(
            logger,
            LogLevel.Debug,
            eventId: default,
            exception,
            message,
            CreateProperty(value, valueExpression));
    }

    /// <summary>
    /// Registra uma mensagem com um único argumento. Se a mensagem não tiver placeholders,
    /// o argumento é enviado como propriedade estruturada usando o nome capturado da expressão.
    /// </summary>
    public static void LogTrace(
        this ILogger logger,
        string? message,
        object? value,
        [CallerArgumentExpression("value")] string? valueExpression = null)
    {
        if (ShouldUseMicrosoftTemplateLogging(message))
        {
            LoggerExtensions.LogTrace(logger, message, value);
            return;
        }

        Log(logger, LogLevel.Trace, message, CreateProperty(value, valueExpression));
    }

    /// <summary>
    /// Registra uma exceção com um único argumento extra. Se a mensagem não tiver placeholders,
    /// o argumento é enviado como propriedade estruturada usando o nome capturado da expressão.
    /// </summary>
    public static void LogTrace(
        this ILogger logger,
        string? message,
        object? value,
        Exception exception,
        [CallerArgumentExpression("value")] string? valueExpression = null)
    {
        if (ShouldUseMicrosoftTemplateLogging(message))
        {
            LoggerExtensions.LogTrace(logger, exception, message, value);
            return;
        }

        Log(
            logger,
            LogLevel.Trace,
            eventId: default,
            exception,
            message,
            CreateProperty(value, valueExpression));
    }

    private static bool ShouldUseMicrosoftTemplateLogging(string? message) =>
        !string.IsNullOrEmpty(message)
        && message.IndexOf('{') >= 0
        && message.IndexOf('}') > message.IndexOf('{');

    private static (string Key, object? Value) CreateProperty(object? value, string? valueExpression)
    {
        if (TryResolveExplicitProperty(value, out var explicitProperty))
            return explicitProperty;

        return (TryResolvePropertyName(valueExpression), value);
    }

    private static bool TryResolveExplicitProperty(
        object? value,
        out (string Key, object? Value) property)
    {
        if (value is ITuple tuple
            && tuple.Length == 2
            && tuple[0] is string key
            && !string.IsNullOrWhiteSpace(key))
        {
            property = (key, tuple[1]);
            return true;
        }

        property = default;
        return false;
    }

    private static string TryResolvePropertyName(string? valueExpression)
    {
        if (string.IsNullOrWhiteSpace(valueExpression))
            return "value";

        var trimmed = valueExpression.Trim();
        var separators = new[] { '.', '?', '!', '[', '(', ' ' };
        var lastSeparatorIndex = trimmed.LastIndexOfAny(separators);
        var candidate = lastSeparatorIndex >= 0 ? trimmed[(lastSeparatorIndex + 1)..] : trimmed;
        candidate = candidate.TrimEnd(')', ']', ';');

        if (string.IsNullOrWhiteSpace(candidate))
            return "value";

        if (!IsSimpleIdentifier(candidate))
            return "value";

        return candidate;
    }

    private static bool IsSimpleIdentifier(string candidate)
    {
        if (candidate.Length == 0)
            return false;

        if (!(char.IsLetter(candidate[0]) || candidate[0] == '_'))
            return false;

        for (var i = 1; i < candidate.Length; i++)
        {
            var ch = candidate[i];
            if (!(char.IsLetterOrDigit(ch) || ch == '_'))
                return false;
        }

        return true;
    }

    private sealed class LogCenterPropertyState : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private const string OriginalFormatKey = "{OriginalFormat}";
        private readonly KeyValuePair<string, object?>[] _pairs;

        private LogCenterPropertyState(string message, KeyValuePair<string, object?>[] pairs)
        {
            Message = message;
            _pairs = pairs;
        }

        public string Message { get; }

        public int Count => _pairs.Length;

        public KeyValuePair<string, object?> this[int index] => _pairs[index];

        public static LogCenterPropertyState Create(
            string? message,
            (string Key, object? Value)[] properties)
        {
            var renderedMessage = message ?? string.Empty;
            var pairs = new KeyValuePair<string, object?>[properties.Length + 1];

            for (var i = 0; i < properties.Length; i++)
            {
                var (key, value) = properties[i];
                ValidatePropertyName(key);
                pairs[i] = new KeyValuePair<string, object?>(key, value);
            }

            pairs[^1] = new KeyValuePair<string, object?>(OriginalFormatKey, renderedMessage);
            return new LogCenterPropertyState(renderedMessage, pairs);
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() =>
            ((IEnumerable<KeyValuePair<string, object?>>)_pairs).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => Message;

        private static void ValidatePropertyName(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Property name cannot be null or whitespace.", nameof(key));

            if (string.Equals(key, OriginalFormatKey, StringComparison.Ordinal)
                || string.Equals(key, "OriginalFormat", StringComparison.Ordinal)
                || string.Equals(key, LogCenterReservedPropertyNames.Timestamp, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Property name '{key}' is reserved.", nameof(key));
            }
        }
    }
}

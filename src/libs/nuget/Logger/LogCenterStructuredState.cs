using System.Text.Json;
using System.Reflection;

namespace LogCenter;

/// <summary>
/// Extrai propriedades do estado estruturado usado por <c>LoggerExtensions.Log*(message, args)</c>.
/// Em templates como <c>{@teste}</c>, o runtime usa a chave literal <c>@teste</c> com o <see cref="object"/> passado ao log.
/// </summary>
internal static class LogCenterStructuredState
{
    private const string OriginalFormatKeyBraced = "{OriginalFormat}";
    private const string OriginalFormatKeyPlain = "OriginalFormat";

    /// <summary>
    /// Constrói um dicionário de propriedades com valores JSON (objetos/arrays aninhados no POST, não string escapada).
    /// </summary>
    public static Dictionary<string, JsonElement>? TryBuildStructuredProperties<TState>(
        TState state,
        JsonSerializerOptions jsonOptions,
        bool stripDestructuringAtPrefix)
    {
        if (state is not IEnumerable<KeyValuePair<string, object?>> pairs)
            return null;

        Dictionary<string, JsonElement>? dict = null;
        foreach (var kv in pairs)
        {
            if (string.Equals(kv.Key, OriginalFormatKeyBraced, StringComparison.Ordinal)
                || string.Equals(kv.Key, OriginalFormatKeyPlain, StringComparison.Ordinal))
                continue;

            var key = ResolvePropertyKey(kv.Key, stripDestructuringAtPrefix);
            dict ??= new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            dict[key] = SerializeStructuredValue(kv.Value, jsonOptions);
        }

        return dict;
    }

    private static JsonElement SerializeStructuredValue(object? value, JsonSerializerOptions jsonOptions)
    {
        if (value is JsonElement jsonElement)
            return jsonElement.Clone();

        try
        {
            return JsonSerializer.SerializeToElement(
                value,
                value?.GetType() ?? typeof(object),
                jsonOptions);
        }
        catch (NotSupportedException)
        {
            var safeValue = ToSafeValue(
                value,
                depth: 0,
                seen: new HashSet<object>(ReferenceEqualityComparer.Instance));

            if (safeValue is JsonElement safeJsonElement)
                return safeJsonElement.Clone();

            return JsonSerializer.SerializeToElement(
                safeValue,
                safeValue?.GetType() ?? typeof(object),
                jsonOptions);
        }
    }

    private static object? ToSafeValue(object? value, int depth, HashSet<object> seen)
    {
        if (value is null)
            return null;

        if (depth >= 6)
            return value.ToString();

        if (value is JsonElement jsonElement)
            return jsonElement.Clone();

        var type = value.GetType();
        if (IsScalar(type))
            return value;

        if (!type.IsValueType && !seen.Add(value))
            return "<cyclic reference>";

        try
        {
            if (value is Type runtimeType)
                return runtimeType.FullName ?? runtimeType.Name;

            if (value is MemberInfo memberInfo)
                return memberInfo.ToString();

            if (value is Exception exception)
                return new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["Type"] = exception.GetType().FullName ?? exception.GetType().Name,
                    ["Message"] = exception.Message,
                    ["Source"] = exception.Source,
                    ["HResult"] = exception.HResult,
                    ["StackTrace"] = exception.StackTrace,
                    ["InnerException"] = ToSafeValue(exception.InnerException, depth + 1, seen)
                };

            if (value is System.Collections.IDictionary dictionary)
            {
                var result = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (System.Collections.DictionaryEntry entry in dictionary)
                {
                    var key = entry.Key?.ToString() ?? "<null>";
                    result[key] = ToSafeValue(entry.Value, depth + 1, seen);
                }

                return result;
            }

            if (value is System.Collections.IEnumerable enumerable && value is not string)
            {
                var items = new List<object?>();
                foreach (var item in enumerable)
                    items.Add(ToSafeValue(item, depth + 1, seen));

                return items;
            }

            var properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(static property => property.CanRead && property.GetIndexParameters().Length == 0);

            var objectResult = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var property in properties)
            {
                object? propertyValue;
                try
                {
                    propertyValue = property.GetValue(value);
                }
                catch (Exception ex)
                {
                    propertyValue = $"<{ex.GetType().Name}: {ex.Message}>";
                }

                objectResult[property.Name] = ToSafeValue(propertyValue, depth + 1, seen);
            }

            if (objectResult.Count > 0)
                return objectResult;

            return value.ToString();
        }
        finally
        {
            if (!type.IsValueType)
                seen.Remove(value);
        }
    }

    private static bool IsScalar(Type type)
    {
        if (type.IsPrimitive || type.IsEnum)
            return true;

        return type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(Guid)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(Uri);
    }

    private static string ResolvePropertyKey(string templateKey, bool stripDestructuringAtPrefix)
    {
        if (!stripDestructuringAtPrefix || !templateKey.StartsWith('@'))
            return templateKey;

        var without = templateKey.Length > 1 ? templateKey[1..] : templateKey;
        return string.IsNullOrEmpty(without) ? templateKey : without;
    }
}

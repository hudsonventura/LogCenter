using System.Text.Json;

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
            dict[key] = JsonSerializer.SerializeToElement(
                kv.Value,
                kv.Value?.GetType() ?? typeof(object),
                jsonOptions);
        }

        return dict;
    }

    private static string ResolvePropertyKey(string templateKey, bool stripDestructuringAtPrefix)
    {
        if (!stripDestructuringAtPrefix || !templateKey.StartsWith("@", StringComparison.Ordinal))
            return templateKey;

        var without = templateKey.Length > 1 ? templateKey[1..] : templateKey;
        return string.IsNullOrEmpty(without) ? templateKey : without;
    }
}

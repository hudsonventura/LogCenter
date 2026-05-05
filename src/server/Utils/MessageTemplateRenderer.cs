using System.Text.Json;
using System.Text.RegularExpressions;

namespace server.Utils;

internal static partial class MessageTemplateRenderer
{
    private static readonly HashSet<string> HiddenPropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "TraceId",
        "_tags_"
    };

    public sealed record RenderResult(string Message, bool AllContentParametersMatched);

    public static string Render(string message, object? content)
    {
        return RenderWithMetadata(message, content).Message;
    }

    public static RenderResult RenderWithMetadata(string message, object? content)
    {
        if (string.IsNullOrWhiteSpace(message) || content is null)
            return new RenderResult(message, false);

        using var document = TryCreateDocument(content);
        if (document is null || document.RootElement.ValueKind != JsonValueKind.Object)
            return new RenderResult(message, false);

        var matchedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var renderedMessage = TemplateTokenRegex().Replace(message, match =>
        {
            var name = match.Groups["name"].Value;

            if (ShouldHideProperty(name))
                return string.Empty;

            if (!TryGetProperty(document.RootElement, name, out var value))
                return match.Value;

            if (!TryFormatSimpleValue(value, out var formatted))
                return match.Value;

            matchedProperties.Add(name);
            return formatted;
        });

        return new RenderResult(
            renderedMessage,
            AllContentPropertiesMatched(document.RootElement, matchedProperties)
        );
    }

    private static JsonDocument? TryCreateDocument(object content)
    {
        try
        {
            if (content is JsonElement element)
                return JsonDocument.Parse(element.GetRawText());

            if (content is string text)
                return string.IsNullOrWhiteSpace(text) ? null : JsonDocument.Parse(text);

            return JsonDocument.Parse(JsonSerializer.Serialize(content));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool TryGetProperty(JsonElement root, string name, out JsonElement value)
    {
        if (root.TryGetProperty(name, out value))
            return true;

        foreach (var property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static bool TryFormatSimpleValue(JsonElement value, out string formatted)
    {
        formatted = string.Empty;

        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                formatted = value.GetString() ?? string.Empty;
                return true;
            case JsonValueKind.Number:
                formatted = value.GetRawText();
                return true;
            case JsonValueKind.True:
                formatted = bool.TrueString.ToLowerInvariant();
                return true;
            case JsonValueKind.False:
                formatted = bool.FalseString.ToLowerInvariant();
                return true;
            case JsonValueKind.Null:
                formatted = "null";
                return true;
            case JsonValueKind.Array:
                return TryFormatSimpleArray(value, out formatted);
            default:
                return false;
        }
    }

    private static bool TryFormatSimpleArray(JsonElement value, out string formatted)
    {
        var items = new List<string>();

        foreach (var item in value.EnumerateArray())
        {
            if (!TryFormatSimpleValue(item, out var itemText) ||
                item.ValueKind is JsonValueKind.Array or JsonValueKind.Object)
            {
                formatted = string.Empty;
                return false;
            }

            items.Add(itemText);
        }

        formatted = string.Join(", ", items);
        return true;
    }

    private static bool AllContentPropertiesMatched(
        JsonElement root,
        HashSet<string> matchedProperties)
    {
        var propertyCount = 0;

        foreach (var property in root.EnumerateObject())
        {
            if (ShouldHideProperty(property.Name))
                continue;

            propertyCount++;

            if (!matchedProperties.Contains(property.Name) ||
                !TryFormatSimpleValue(property.Value, out _))
            {
                return false;
            }
        }

        return propertyCount > 0;
    }

    private static bool ShouldHideProperty(string name)
    {
        var normalizedName = name.StartsWith('@') ? name[1..] : name;
        return HiddenPropertyNames.Contains(normalizedName);
    }

    [GeneratedRegex(@"\{(?<name>@?[A-Za-z_][A-Za-z0-9_.-]*)\}")]
    private static partial Regex TemplateTokenRegex();
}

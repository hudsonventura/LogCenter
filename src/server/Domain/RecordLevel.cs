using System.Text.Json;
using System.Text.Json.Serialization;

namespace server.Domain;

[JsonConverter(typeof(RecordLevelJsonConverter))]
public enum RecordLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    None = 6,


    HttpRequest = 99,
    HttpResponse200 = 200,
    HttpResponse201 = 201,
    HttpResponse202 = 202,
    HttpResponse204 = 204,

    HttpResponse301 = 301,
    HttpResponse302 = 302,
    HttpResponse304 = 304,

    HttpResponse400 = 400,
    HttpResponse401 = 401,
    HttpResponse403 = 403,
    HttpResponse404 = 404,
    HttpResponse405 = 405,
    HttpResponse408 = 408,
    HttpResponse409 = 409,
    HttpResponse422 = 422,
    HttpResponse429 = 429,

    HttpResponse500 = 500,
    HttpResponse501 = 501,
    HttpResponse502 = 502,
    HttpResponse503 = 503,
    HttpResponse504 = 504
}

public static class RecordLevelMapper
{
    public static RecordLevel FromMicrosoft(Microsoft.Extensions.Logging.LogLevel level) =>
        level switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => RecordLevel.Trace,
            Microsoft.Extensions.Logging.LogLevel.Debug => RecordLevel.Debug,
            Microsoft.Extensions.Logging.LogLevel.Information => RecordLevel.Information,
            Microsoft.Extensions.Logging.LogLevel.Warning => RecordLevel.Warning,
            Microsoft.Extensions.Logging.LogLevel.Error => RecordLevel.Error,
            Microsoft.Extensions.Logging.LogLevel.Critical => RecordLevel.Critical,
            Microsoft.Extensions.Logging.LogLevel.None => RecordLevel.None,
            _ => RecordLevel.Information
        };
}

internal sealed class RecordLevelJsonConverter : JsonConverter<RecordLevel>
{
    public override RecordLevel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number when reader.TryGetInt32(out var numericLevel)
                => (RecordLevel)numericLevel,
            JsonTokenType.String => ParseStringLevel(reader.GetString()),
            _ => throw new JsonException("Invalid log level value.")
        };
    }

    public override void Write(Utf8JsonWriter writer, RecordLevel value, JsonSerializerOptions options) =>
        writer.WriteNumberValue((int)value);

    private static RecordLevel ParseStringLevel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new JsonException("Log level cannot be empty.");

        if (Enum.TryParse<RecordLevel>(value, ignoreCase: true, out var level))
            return level;

        throw new JsonException($"Unknown log level '{value}'.");
    }
}

using System.Text.Json;

namespace LogCenter;

/// <summary>Corpo JSON enviado ao endpoint remoto.</summary>
internal sealed class LogCenterLogPayload
{
    public DateTimeOffset Timestamp { get; set; }
    public string Level { get; set; } = "";
    public string Category { get; set; } = "";
    public string Message { get; set; } = "";
    public string? Exception { get; set; }
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Valores dos buracos do template (ex.: <c>{@teste}</c> → referência ao objeto em runtime; aqui como JSON aninhado).
    /// Com <see cref="LogCenterOptions.StripDestructuringAtPrefix"/>, a chave enviada pode ser <c>teste</c> em vez de <c>@teste</c>.
    /// </summary>
    public Dictionary<string, JsonElement>? StructuredProperties { get; set; }
}

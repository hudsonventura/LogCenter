
using Microsoft.Extensions.Logging;

namespace LogCenter;

/// <summary>
/// Opções para envio de logs via HTTP (seção de configuração padrão: "LogCenter").
/// </summary>
public class LogCenterOptions
{
    /// <summary>Nível mínimo registrado por este provider.</summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Trace;


    /// <summary>Nome da aplicação incluído no corpo JSON (opcional).</summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Quando true, nomes de propriedade que começam com <c>@</c> (template <c>{@x}</c>) são enviados sem o <c>@</c> (ex.: <c>teste</c>).
    /// O runtime continua a usar a chave <c>@teste</c> internamente; isto só altera o JSON do POST.
    /// </summary>
    public bool StripDestructuringAtPrefix { get; set; } = true;



    /// <summary>
    /// LogCenter's URL
    /// </summary>
    public string Url { get ; set; } = string.Empty;

    /// <summary>
    /// Table name
    /// </summary>
    public string Table { get ; set; } = string.Empty;


    /// <summary>
    /// Your token. Generate it on web interface on user's preference
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Timeout for HTTP requests, in seconds. Default is 5 seconds (5000 ms).
    /// </summary>    
    public int Timeout { get; set; } = 5000;

    /// <summary>
    /// Event ids that should be ignored by the logger provider.
    /// </summary>
    public List<int> BannedEventIds { get; set; } = new();

    /// <summary>
    /// Event names that should be ignored by the logger provider.
    /// </summary>
    public List<string> BannedEventNames { get; set; } = new();

    /// <summary>
    /// Message templates or messages that should be ignored by the logger provider.
    /// </summary>
    public List<string> BannedMessages { get; set; } = new();
}

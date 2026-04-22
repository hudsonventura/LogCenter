
using Microsoft.Extensions.Logging;

namespace LogCenter;

/// <summary>
/// Opções para envio de logs via HTTP (seção de configuração padrão: "LogFull").
/// </summary>
public class LogCenterOptions
{

    /// <summary>Quando false, o provider não envia requisições (útil para testes ou ambiente local).</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>URL base do serviço que recebe os logs (ex.: https://logs.exemplo.com).</summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>Caminho relativo ao enviar POST (ex.: /api/logs).</summary>
    public string LogEndpoint { get; set; } = "/api/logs";


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
}

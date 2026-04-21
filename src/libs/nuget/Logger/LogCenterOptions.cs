
using Microsoft.Extensions.Logging;

namespace LogCenter;

/// <summary>
/// Opções para envio de logs via HTTP (seção de configuração padrão: "LogFull").
/// </summary>
public sealed class LogCenterOptions
{
    public const string SectionName = "LogFull";

    /// <summary>Nome do <see cref="HttpClient"/> registrado em <c>AddHttpClient</c>.</summary>
    public const string HttpClientName = "LogCenter";

    /// <summary>Quando false, o provider não envia requisições (útil para testes ou ambiente local).</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>URL base do serviço que recebe os logs (ex.: https://logs.exemplo.com).</summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>Caminho relativo ao enviar POST (ex.: /api/logs).</summary>
    public string LogEndpoint { get; set; } = "/api/logs";

    /// <summary>Timeout por requisição.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Nível mínimo registrado por este provider.</summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Trace;

    /// <summary>Nome do header para API key (opcional).</summary>
    public string? ApiKeyHeaderName { get; set; }

    /// <summary>Valor da API key (opcional).</summary>
    public string? ApiKey { get; set; }

    /// <summary>Nome da aplicação incluído no corpo JSON (opcional).</summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Quando true, nomes de propriedade que começam com <c>@</c> (template <c>{@x}</c>) são enviados sem o <c>@</c> (ex.: <c>teste</c>).
    /// O runtime continua a usar a chave <c>@teste</c> internamente; isto só altera o JSON do POST.
    /// </summary>
    public bool StripDestructuringAtPrefix { get; set; } = true;

    /// <summary>
    /// Quando true, o método de log bloqueia até o POST terminar (útil para depuração).
    /// Por defeito o envio é assíncrono (fire-and-forget) e o depurador pode não parar dentro do envio HTTP.
    /// Evite em produção: bloqueia threads e pode causar deadlock se o endpoint de logs voltar a invocar esta aplicação.
    /// </summary>
    public bool SendSynchronously { get; set; }


    /// <summary>
    /// LogCenter's URL
    /// </summary>
    public string url { get ; set; } = string.Empty;

    /// <summary>
    /// Table name
    /// </summary>
    public string table { get ; set; } = string.Empty;


    /// <summary>
    /// Your token. Generate it on web interface on user's preference
    /// </summary>
    public string token { get; set; } = string.Empty;
}

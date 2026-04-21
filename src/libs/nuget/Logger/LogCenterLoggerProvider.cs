using Microsoft.Extensions.Logging;

namespace LogCenter;

internal sealed class LogCenterLoggerProvider : ILoggerProvider
{
    private readonly LogCenterOptions _options;
    private readonly HttpClient _httpClient;

    public LogCenterLoggerProvider(LogCenterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        
        // Cria o HttpClient internamente com as opções fornecidas
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_options.url),
            Timeout = _options.Timeout
        };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.token}");
    }

    public ILogger CreateLogger(string categoryName) =>
        new LogCenterLogger(categoryName, _httpClient, _options);

    public void Dispose() => _httpClient?.Dispose();
}

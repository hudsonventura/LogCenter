using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogCenter;

internal sealed class LogCenterLoggerProvider : ILoggerProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<LogCenterOptions> _options;

    public LogCenterLoggerProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<LogCenterOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public ILogger CreateLogger(string categoryName) =>
        new LogCenterLogger(categoryName, _httpClientFactory, _options.Value);

    public void Dispose() { }
}

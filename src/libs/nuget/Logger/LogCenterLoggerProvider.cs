using Microsoft.Extensions.Logging;

namespace LogCenter;

internal sealed class LogCenterLoggerProvider : ILoggerProvider
{
    private readonly LogCenterOptions _options;
    private readonly HttpClient _httpClient;
    private readonly LogCenterPendingSendTracker _pendingSendTracker = new();

    public LogCenterLoggerProvider(LogCenterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_options.Url),
            Timeout = TimeSpan.FromMilliseconds(_options.Timeout)
        };

        if (!string.IsNullOrWhiteSpace(_options.Token))
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.Token}");
    }

    public ILogger CreateLogger(string categoryName) =>
        new LogCenterLogger(categoryName, _httpClient, _options, _pendingSendTracker);

    public void Dispose()
    {
        _pendingSendTracker.WaitForPendingSendsAsync().GetAwaiter().GetResult();
        _httpClient.Dispose();
    }


}

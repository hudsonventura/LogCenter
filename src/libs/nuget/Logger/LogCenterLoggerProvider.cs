using Microsoft.Extensions.Logging;

namespace LogCenter;

internal sealed class LogCenterLoggerProvider : ILoggerProvider
{
    private readonly LogCenterOptions _options;
    private readonly HttpClient _httpClient;
    private readonly LogCenterDrain _drain;

    public LogCenterLoggerProvider(LogCenterOptions options, LogCenterDrain drain)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _drain = drain ?? throw new ArgumentNullException(nameof(drain));

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_options.Url),
            Timeout = TimeSpan.FromMilliseconds(_options.Timeout)
        };

        if (!string.IsNullOrWhiteSpace(_options.Token))
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.Token}");
    }

    public ILogger CreateLogger(string categoryName) =>
        new LogCenterLogger(categoryName, _httpClient, _options, _drain.Tracker);

    public void Dispose() => _httpClient.Dispose();


}

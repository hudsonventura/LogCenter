using Microsoft.Extensions.Logging;

namespace LogCenter;

internal sealed class LogCenterLoggerProvider : ILoggerProvider
{
    private readonly LogCenterOptions _options;
    private readonly HttpClient _httpClient;

    public LogCenterLoggerProvider(LogCenterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        _httpClient = new HttpClient
        {
            BaseAddress = ResolveBaseAddress(_options),
            Timeout = TimeSpan.FromMilliseconds(_options.Timeout)
        };

        if (!string.IsNullOrWhiteSpace(_options.Token))
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.Token}");
    }

    public ILogger CreateLogger(string categoryName) =>
        new LogCenterLogger(categoryName, _httpClient, _options);

    public void Dispose() => _httpClient?.Dispose();

    private static Uri? ResolveBaseAddress(LogCenterOptions options)
    {
        if (options.BaseAddress is not null)
            return options.BaseAddress;

        if (!string.IsNullOrWhiteSpace(options.Url))
            return new Uri(options.Url, UriKind.Absolute);

        if (options.Enabled)
            throw new InvalidOperationException("LogCenterOptions.Url or LogCenterOptions.BaseAddress is required when logging is enabled.");

        return null;
    }
}

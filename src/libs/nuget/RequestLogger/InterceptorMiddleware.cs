using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogCenter.RequestInterceptor;

public static class InterceptorMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestInterceptor(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var options = ResolveOptions(builder.ApplicationServices);
        var httpClient = CreateHttpClient(options);

        return builder.UseMiddleware<InterceptorMiddleware>(options, httpClient);
    }

    public static IApplicationBuilder UseRequestInterceptor(this IApplicationBuilder builder, InterceptorOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        var httpClient = CreateHttpClient(options);

        return builder.UseMiddleware<InterceptorMiddleware>(options, httpClient);
    }

    public static IApplicationBuilder UseRequestInterceptor(this IApplicationBuilder builder, LogCenterOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        var interceptorOptions = ToInterceptorOptions(options);
        var httpClient = CreateHttpClient(interceptorOptions);

        return builder.UseMiddleware<InterceptorMiddleware>(interceptorOptions, httpClient);
    }

    private static InterceptorOptions ResolveOptions(IServiceProvider services)
    {
        var interceptorOptions = services.GetService<InterceptorOptions>();
        if (interceptorOptions is not null)
            return interceptorOptions;

        var logCenterOptions = services.GetRequiredService<LogCenterOptions>();
        return logCenterOptions as InterceptorOptions ?? ToInterceptorOptions(logCenterOptions);
    }

    private static InterceptorOptions ToInterceptorOptions(LogCenterOptions options) =>
        new()
        {
            MinimumLevel = options.MinimumLevel,
            ApplicationName = options.ApplicationName,
            StripDestructuringAtPrefix = options.StripDestructuringAtPrefix,
            Url = options.Url,
            Table = options.Table,
            Token = options.Token,
            Timeout = options.Timeout
        };

    private static HttpClient CreateHttpClient(LogCenterOptions options)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(options.Url),
            Timeout = TimeSpan.FromMilliseconds(options.Timeout)
        };

        if (!string.IsNullOrWhiteSpace(options.Token))
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.Token.Trim()}");

        return httpClient;
    }


}

public sealed class InterceptorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpClient _httpClient;
    private readonly InterceptorOptions _options;

    public InterceptorMiddleware(
        RequestDelegate next,
        InterceptorOptions options,
        HttpClient httpClient)
    {
        _next = next;
        _options = options;
        _httpClient = httpClient;
    }

    public async Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var requestStartedAt = DateTimeOffset.UtcNow;

        Request request = await Request.Convert(context);
        string traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        if (_options.LogGetRequest || !HttpMethods.IsGet(request.Method))
        {
            QueueSendRequest(request, traceId, requestStartedAt);
        }

        context.Response.Headers[_options.TraceIdReponseHeader] = traceId;

        Stream originalbody = context.Response.Body;
        string responseBody = string.Empty;

        await using var memstream = new MemoryStream();
        context.Response.Body = memstream;

        try
        {
            await _next(context);
            var responseCompletedAt = DateTimeOffset.UtcNow;

            memstream.Position = 0;
            responseBody = await new StreamReader(memstream).ReadToEndAsync();
            memstream.Position = 0;
            await memstream.CopyToAsync(originalbody);
            context.Response.Body = originalbody;

            Response response = await Response.Convert(context, responseBody);

            if (_options.LogGetRequest || !HttpMethods.IsGet(request.Method))
            {
                QueueSendResponse(response, traceId, responseCompletedAt);
            }
        }
        catch (Exception e)
        {
            var responseCompletedAt = DateTimeOffset.UtcNow;
            memstream.Position = 0;
            responseBody = await new StreamReader(memstream).ReadToEndAsync();
            memstream.Position = 0;
            await memstream.CopyToAsync(originalbody);
            context.Response.Body = originalbody;
            context.Response.StatusCode = 500;

            Response response = await Response.Convert(context, e);
            QueueSendResponse(response, traceId, responseCompletedAt);

            //I don't remeber for what reason I added this HideResponseExceptions option
            //if (!_options.HideResponseExceptions)
            //{
            //    await context.Response.WriteAsync(e.ToString());
            //}
        }
    }

    public Task OnReceiveRequestAsync(Request request, string traceId, DateTimeOffset timestamp)
    {
        object payload = _options.FormatType switch
        {
            InterceptorOptions.SaveFormatType.HTTPText => request.ToString(),
            _ => request
        };

        return SendAsync(new RequestRecord
        {
            Message = "HTTP request received",
            Category = LogCategory.HttpRequest,
            Timestamp = timestamp.UtcDateTime,
            Level = ResolveLevel(LogLevel.Trace),
            TraceId = traceId,
            Content = payload
        });
    }

    public Task OnSendResponseAsync(Response response, string traceId, DateTimeOffset timestamp)
    {
        object payload = _options.FormatType switch
        {
            InterceptorOptions.SaveFormatType.HTTPText => response.ToString(),
            _ => response
        };

        return SendAsync(new RequestRecord
        {
            Message = "HTTP response sent",
            Category = LogCategory.HttpResponse,
            Timestamp = timestamp.UtcDateTime,
            Level = ResolveLevel(MapLogLevel(response.StatusCode)),
            TraceId = traceId,
            Content = payload
        });
    }

    private async Task SendAsync(RequestRecord payload)
    {
        if (!IsEnabled(payload.Level) || _httpClient.BaseAddress is null)
            return;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, ResolveRequestUri())
            {
                Content = JsonContent.Create(payload)
            };

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LogCenter.RequestInterceptor] Fail sending request log: {ex.Message}");
        }
    }

    private void QueueSendRequest(Request request, string traceId, DateTimeOffset timestamp)
    {
        _ = Task.Run(async () =>
        {
            await OnReceiveRequestAsync(request, traceId, timestamp).ConfigureAwait(false);
        });
    }

    private void QueueSendResponse(Response response, string traceId, DateTimeOffset timestamp)
    {
        _ = Task.Run(async () =>
        {
            await OnSendResponseAsync(response, traceId, timestamp).ConfigureAwait(false);
        });
    }

    private bool IsEnabled(int level) =>
        level >= ResolveLevel(_options.MinimumLevel);

    private string ResolveRequestUri()
    {
        var table = _options.Table
            .Trim()
            .Trim('/')
            .Replace(" ", "_", StringComparison.Ordinal)
            .ToLowerInvariant();

        return "/" + table;
    }

    private static int ResolveLevel(LogLevel level) =>
        level switch
        {
            LogLevel.Trace => 0,
            LogLevel.Debug => 2,
            LogLevel.Information => 1,
            LogLevel.Warning => 3,
            LogLevel.Error => 4,
            LogLevel.Critical => 5,
            _ => 1
        };

    private static LogLevel MapLogLevel(int statusCode) =>
        statusCode switch
        {
            >= 500 => LogLevel.Critical,
            >= 400 => LogLevel.Error,
            >= 300 => LogLevel.Warning,
            _ => LogLevel.Information
        };
}

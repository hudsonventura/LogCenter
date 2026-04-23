using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

//using RequestResponseInterceptor;
//using RequestResponseInterceptor.Implementations;



namespace LogCenter.RequestInterceptor;

public static class InterceptorMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestInterceptor(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var options = ResolveOptions(builder.ApplicationServices);

        return builder.UseMiddleware<InterceptorMiddleware>(options);
    }

    public static IApplicationBuilder UseRequestInterceptor(this IApplicationBuilder builder, InterceptorOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        return builder.UseMiddleware<InterceptorMiddleware>(options);
    }

    public static IApplicationBuilder UseRequestInterceptor(this IApplicationBuilder builder, LogCenterOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        return builder.UseRequestInterceptor(ToInterceptorOptions(options));
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
            Enabled = options.Enabled,
            BaseAddress = options.BaseAddress,
            LogEndpoint = options.LogEndpoint,
            MinimumLevel = options.MinimumLevel,
            ApplicationName = options.ApplicationName,
            StripDestructuringAtPrefix = options.StripDestructuringAtPrefix,
            Url = options.Url,
            Table = options.Table,
            Token = options.Token,
            Timeout = options.Timeout
        };
}


public sealed class InterceptorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InterceptorMiddleware> _logger;
    private readonly InterceptorOptions _options;

    public InterceptorMiddleware(
        RequestDelegate next,
        InterceptorOptions options,
        ILogger<InterceptorMiddleware> logger)
    {
        _next = next;
        _options = options;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Request request = await Request.Convert(context);
        string traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        if (_options.LogGetRequest || !HttpMethods.IsGet(request.Method))
        {
            OnReceiveRequest(request, traceId);
        }

        context.Response.Headers[_options.TraceIdReponseHeader] = traceId;

        Stream originalbody = context.Response.Body;
        string responseBody = "";

        using (var memstream = new MemoryStream())
        {
            context.Response.Body = memstream;

            try
            {
                await _next(context);

                memstream.Position = 0;
                responseBody = await new StreamReader(memstream).ReadToEndAsync();
                memstream.Position = 0;
                await memstream.CopyToAsync(originalbody);
                context.Response.Body = originalbody;

                Response response = await Response.Convert(context, responseBody);

                if (_options.LogGetRequest || !HttpMethods.IsGet(request.Method))
                {
                    OnSendResponse(response, traceId);
                }
            }
            catch (Exception e)
            {
                memstream.Position = 0;
                responseBody = await new StreamReader(memstream).ReadToEndAsync();
                memstream.Position = 0;
                await memstream.CopyToAsync(originalbody);
                context.Response.Body = originalbody;
                context.Response.StatusCode = 500;

                Response response = await Response.Convert(context, e);
                OnSendResponse(response, traceId);

                if (!_options.HideResponseExceptions)
                {
                    await context.Response.WriteAsync(e.ToString());
                }
            }
        }
    }

    public void OnReceiveRequest(Request request, string traceId)
    {
        object payload = _options.FormatType switch
        {
            InterceptorOptions.SaveFormatType.HTTPText => request.ToString(),
            _ => request
        };

        LogStructured(
            LogLevel.Trace,
            new EventId(1000, nameof(Request)),
            "HTTP request received",
            traceId,
            nameof(Request),
            payload);
    }

    public void OnSendResponse(Response response, string traceId)
    {
        object payload = _options.FormatType switch
        {
            InterceptorOptions.SaveFormatType.HTTPText => response.ToString(),
            _ => response
        };

        LogStructured(
            MapLogLevel(response.StatusCode),
            new EventId(1001, nameof(Response)),
            "HTTP response sent",
            traceId,
            nameof(Response),
            payload,
            response.Exception);
    }

    private void LogStructured(
        LogLevel level,
        EventId eventId,
        string message,
        string traceId,
        string propertyName,
        object payload,
        Exception? exception = null)
    {
        if (!_logger.IsEnabled(level))
            return;

        var state = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["{OriginalFormat}"] = message,
            ["TraceId"] = traceId,
            [propertyName] = payload
        };

        _logger.Log(
            level,
            eventId,
            state,
            exception,
            static (currentState, _) =>
                currentState.TryGetValue("{OriginalFormat}", out var currentMessage)
                    ? currentMessage?.ToString() ?? string.Empty
                    : string.Empty);
    }

    private static LogLevel MapLogLevel(int statusCode) =>
        statusCode switch
        {
            >= 500 => LogLevel.Critical,
            >= 400 => LogLevel.Error,
            >= 300 => LogLevel.Warning,
            _ => LogLevel.Information
        };
}

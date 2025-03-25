using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

//using RequestResponseInterceptor;
//using RequestResponseInterceptor.Implementations;



namespace LogCenter.RequestInterceptor;

public static class InterceptorMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestInterceptor(this IApplicationBuilder builder)
    {
        var options = builder.ApplicationServices.GetRequiredService<LogCenterOptions>();

        return builder.UseMiddleware<InterceptorMiddleware>(options);
    }

     public static IApplicationBuilder UseRequestInterceptor(this IApplicationBuilder builder,  LogCenterOptions options)
    {
        return builder.UseMiddleware<InterceptorMiddleware>(options);
    }
}


public sealed class InterceptorMiddleware
{
    private readonly RequestDelegate _next;
    private LogCenterLogger _logger;
    private InterceptorOptions _options;

    public InterceptorMiddleware(RequestDelegate next, InterceptorOptions options)
    {
        _next = next;
        _options = options;
        _logger = new LogCenterLogger(options);
    }

    public async Task Invoke(HttpContext context)
    {
        Request request = await Request.Convert(context);
        string traceId = Activity.Current?.Id ?? context?.TraceIdentifier;

        if(_options.LogGetRequest == false && request.Method == "GET"){
            
        }else{
            OnReceiveRequest(request, traceId);
        }
        

        context.Response.Headers.Add(_options.TraceIdReponseHeader, traceId); // Garantir que o header seja adicionado

        // Salva o body original da response
        Stream originalbody = context.Response.Body;
        string response_body = "";

        using (var memstream = new MemoryStream())
        {
            context.Response.Body = memstream;

            Exception error = null;
            try
            {
                await _next(context); // Continua o pipeline

                // Captura e restaura o body da response
                memstream.Position = 0;
                response_body = new StreamReader(memstream).ReadToEnd();
                memstream.Position = 0;
                await memstream.CopyToAsync(originalbody);
                context.Response.Body = originalbody;

                Response response = await Response.Convert(context, response_body);

                if(_options.LogGetRequest == false && request.Method == "GET"){
            
                }else{
                    OnSendResponse(response, traceId);
                }
                
            }
            catch (Exception e)
            {
                error = e;
                memstream.Position = 0;
                response_body = await new StreamReader(memstream).ReadToEndAsync();
                memstream.Position = 0;
                await memstream.CopyToAsync(originalbody);
                context.Response.Body = originalbody;
                context.Response.StatusCode = 500;

                Response response = await Response.Convert(context, e);
                OnSendResponse(response, traceId);

                // 🔹 Define o status code antes de lançar a exceção
                
                

                if (!_options.HideResponseExceptions)
                {
                    await context.Response.WriteAsync(error.ToString());
                }
            }

        }
    }




    public async void OnReceiveRequest(Request request, string traceId)
    {

        switch (_options.FormatType)
        {
            case InterceptorOptions.SaveFormatType.HTTPText: _logger.Log(LogLevel.Trace, "Request", traceId, request.ToString());
            break;

            default: _logger.Log(LogLevel.Trace, "Request", request);
            break;
        }

    }

    public async void OnSendResponse(Response response, string traceId)
    {      
        try
        {
            LogLevel level = LogLevel.Information;
            switch (response.StatusCode.ToString().Substring(0, 1))
            {
                case "2": level = LogLevel.Success;
                break;

                case "3": level = LogLevel.Warning;
                break;

                case "4": level = LogLevel.Error;
                break;

                case "5": level = LogLevel.Fatal;
                break;

                default: level = LogLevel.Information;
                break;
            }

            switch (_options.FormatType)
            {
                case InterceptorOptions.SaveFormatType.HTTPText: _logger.Log(level, "Response", traceId, response.ToString());
                break;

                default: _logger.Log(level, "Response", response);
                break;
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao fazer a solicitação: {ex.Message}");
        }
    }
    
}
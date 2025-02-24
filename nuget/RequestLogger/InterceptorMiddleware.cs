using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

//using RequestResponseInterceptor;
//using RequestResponseInterceptor.Implementations;



namespace LogCenter.RequestInterceptor;

public static class InterceptorMiddlewareExtensions
{
     public static IApplicationBuilder UseInterceptor(this IApplicationBuilder builder,  LogCenterOptions options)
    {
        return builder.UseMiddleware<InterceptorMiddleware>(options);
    }
}


public sealed class InterceptorMiddleware
{
    private readonly RequestDelegate _next;
    private LogCenterLogger _logger;
    private LogCenterOptions _options;

    public InterceptorMiddleware(RequestDelegate next, LogCenterOptions options)
    {
        _next = next;
        _options = options;
        _logger = new LogCenterLogger(options);
    }

    public async Task Invoke(HttpContext context)
    {
        Request request = await Request.Convert(context);

        if(_options.LogGetRequest == false && request.Method == "GET"){
            
        }else{
            OnReceiveRequest(request);
        }
        

        string traceId = Activity.Current?.Id ?? context?.TraceIdentifier;
        context.Response.Headers.Add("traceId", traceId); // Garantir que o header seja adicionado

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
                    OnSendResponse(response);
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
                OnSendResponse(response);

                // 🔹 Define o status code antes de lançar a exceção
                
                

                if (!_options.HideResponseExceptions)
                {
                    await context.Response.WriteAsync(error.ToString());
                }
            }

        }
    }




    public async void OnReceiveRequest(Request request)
    {

        switch (_options.formatType)
        {
            case LogCenterOptions.SaveFormatType.HTTPText: _logger.LogInformation(request.ToString());
            break;

            default: _logger.LogInformation(request);
            break;
        }

    }

    public async void OnSendResponse(Response response)
    {      
        try
        {
            string levelString = "Info";
            switch (response.StatusCode.ToString().Substring(0, 1))
            {
                case "2": levelString = "Info";
                break;

                case "3": levelString = "Warning";
                break;

                case "4": levelString = "Warning";
                break;

                case "5": levelString = "Error";
                break;

                default: levelString = "Error";
                break;
            }

            switch (_options.formatType)
            {
                case LogCenterOptions.SaveFormatType.HTTPText: _logger.LogInformation(response.ToString());
                break;

                default: _logger.LogInformation(response);
                break;
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao fazer a solicitação: {ex.Message}");
        }
    }
    
}
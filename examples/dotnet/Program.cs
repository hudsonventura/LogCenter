// See https://aka.ms/new-console-template for more information
using LogCenter;
using Microsoft.Extensions.Logging;
//dotnet add package Microsoft.Extensions.Logging.Console

var drain = new LogCenterDrain();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddLogCenter(
        new LogCenterOptions
        {
            Url = "http://localhost:9200",
            Table = "example_console2",
            Token = "eyJhbGciOiJIUzI1NiIsInR5cC..."
        },
        drain);
});

ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

AppDomain.CurrentDomain.UnhandledException += (_, args) =>
{
    if (args.ExceptionObject is Exception exception)
        logger.LogCritical(exception, "Unhandled exception");
    else
        logger.LogCritical("Unhandled exception: {ExceptionObject}", args.ExceptionObject);

    drain.Flush();
};

// Log com objetos estruturados
var test = new { 
    nome = "John Doe Jr. da Silva",
    idade = 30,
    email = "john.doe.jr@example.com"
};





// Log sincronamente com os mesmos métodos do ILogger

logger.LogInformation("Dados do usuário {@user}", test);

logger.LogInformation("Hello World - Trace");
logger.LogDebug("Hello World - Debug");
logger.LogInformation("Hello World - Information");
logger.LogWarning("Hello World - Warning");
logger.LogError("Hello World - Error");
logger.LogCritical("Hello World - Critical {@exception}", new Exception("Test exception"));

throw new Exception("Test exception - unhandled");

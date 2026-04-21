// See https://aka.ms/new-console-template for more information
using LogCenter;
using Microsoft.Extensions.Logging;
//dotnet add package Microsoft.Extensions.Logging.Console


using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddLogCenter(new LogCenterOptions
    {
        url = "http://localhost:9200",
        table = "example_console",
        token = "seu_token_aqui"
    });
});

var logger = loggerFactory.CreateLogger<Program>();



// Log sincronamente com os mesmos métodos do ILogger
logger.LogTrace("Hello World - Trace");
logger.LogDebug("Hello World - Debug");
logger.LogInformation("Hello World - Information");
logger.LogWarning("Hello World - Warning");
logger.LogError("Hello World - Error");
logger.LogCritical("Hello World - Critical");

// Log com objetos estruturados
var test = new { 
    nome = "John Doe Jr. da Silva",
    idade = 30,
    email = "john.doe.jr@example.com"
};

logger.LogInformation("Dados do usuário {@user}", test);
logger.LogCritical("Erro crítico {@error}", test);


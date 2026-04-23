// See https://aka.ms/new-console-template for more information
using LogCenter;
using Microsoft.Extensions.Logging;
//dotnet add package Microsoft.Extensions.Logging.Console


using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddLogCenter(new LogCenterOptions
    {
        Url = "http://localhost:9200",
        Table = "example_console",
        Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AYWRtaW4uY29tIiwibmFtZSI6ImV4YW1wbGVfY29uc29sZSIsInRhYmxlcyI6ImV4YW1wbGVfY29uc29sZSIsImV4cCI6MTc3OTQwMTg5NSwiaXNzIjoiU2V1SXNzdWVyIiwiYXVkIjoiU2V1QXVkaWVuY2UifQ.SOCfxUeUOhOJ3etnYXn71BoDv03C6xXPKXA0ocPqtRU"
    });
});

var logger = loggerFactory.CreateLogger<Program>();





// Log com objetos estruturados
var test = new { 
    nome = "John Doe Jr. da Silva",
    idade = 30,
    email = "john.doe.jr@example.com"
};





// Log sincronamente com os mesmos métodos do ILogger
logger.LogInformation("Hello World - Trace");

logger.LogInformation("Dados do usuário {@user}", test);

logger.LogDebug("Hello World - Debug");
logger.LogInformation("Hello World - Information");
logger.LogWarning("Hello World - Warning");
logger.LogError("Hello World - Error");
logger.LogCritical("Hello World - Critical");
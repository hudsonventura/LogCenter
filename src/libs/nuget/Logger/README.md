# LogCenter.Logger

This package helps you send logs from a .NET console application to LogCenter using the standard `ILogger` abstraction.

See: https://github.com/hudsonventura/LogCenter

![Web Interface](https://github.com/hudsonventura/LogCenter/blob/main/logo.png?raw=true)

### Getting Started

Install the package:

```bash
dotnet add package Microsoft.Extensions.Logging.Console && \
dotnet add package LogCenter.Logger
```


### Basic Setup

The example below is based on [`examples/dotnet/Program.cs`](../../../../examples/dotnet/Program.cs).

```csharp
using LogCenter;
using Microsoft.Extensions.Logging;

var drain = new LogCenterDrain();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddLogCenter(
        new LogCenterOptions
        {
            // LogCenter's URL
            url = "http://localhost:9200",

            // Table name 
            table = "example_interceptor",

            // Generate this on LogCenter inteface, on you profile photo.
            token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",

            // The timeout to send the log and data to LogCenter
            Timeout = 5000
        },
        drain);
});

var logger = loggerFactory.CreateLogger<Program>();
```

### Logging Messages

After configuring the provider, use `ILogger` exactly as you normally would in a console application:

```csharp

var test = new
{
    nome = "John Doe Jr. da Silva",
    idade = 30,
    email = "john.doe.jr@example.com"
};

logger.LogInformation("Dados do usuário {@user}", test);
logger.LogDebug("Hello World - Debug");
logger.LogInformation("Hello World - Information");
logger.LogWarning("Hello World - Warning");
logger.LogError("Hello World - Error");
logger.LogCritical("Hello World - Critical");
```

### Unhandled Exceptions

For console applications, you can log an unhandled exception and wait for the HTTP send to finish before shutdown. The the `drain` at line `var drain = new LogCenterDrain();` and used after, is going to guarantee this functionality.



### Structured Logging

`LogCenter.Logger` supports structured logging with message templates.

Example:

```csharp
var user = new
{
    nome = "John Doe Jr. da Silva",
    idade = 30,
    email = "john.doe.jr@example.com"
};

logger.LogInformation("Dados do usuário {@user}", user);
```

This sends:

- the rendered message
- the log level
- timestamp
- trace information when available
- structured properties as JSON

That makes the data readable in LogCenter and also searchable as JSON.


### Notes

- The package works with the normal `ILogger` API.
- Logs are sent over HTTP to LogCenter.
- Structured objects are serialized and stored in a JSON-friendly way.
- This package is a good fit for console apps, workers, background services, and other non-ASP.NET processes.


### Build

To build the NuGet package locally:

```bash
dotnet pack src/libs/nuget/Logger/Logger.csproj -c Release
```

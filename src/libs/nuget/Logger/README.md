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

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddLogCenter(new LogCenterOptions
    {
        // 
        Url = "http://localhost:9200",

        //
        Table = "example_console",

        //
        Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });
});

var logger = loggerFactory.CreateLogger<Program>();
```

### Logging Messages

After configuring the provider, use `ILogger` exactly as you normally would in a console application:

```csharp
using LogCenter;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddLogCenter(new LogCenterOptions
    {
        Url = "http://localhost:9200",
        Table = "example_console",
        Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });
});

var logger = loggerFactory.CreateLogger<Program>();

var test = new
{
    nome = "John Doe Jr. da Silva",
    idade = 30,
    email = "john.doe.jr@example.com"
};

logger.LogInformation("Hello World - Trace");
logger.LogInformation("Dados do usuário {@user}", test);
logger.LogDebug("Hello World - Debug");
logger.LogInformation("Hello World - Information");
logger.LogWarning("Hello World - Warning");
logger.LogError("Hello World - Error");
logger.LogCritical("Hello World - Critical");
```

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

### LogCenterOptions

Main options for a console app:

```csharp
new LogCenterOptions
{
    Url = "http://localhost:9200",
    Table = "example_console",
    Token = "your-token-here",

    // Optional
    // Enabled = true,
    // MinimumLevel = LogLevel.Trace,
    // ApplicationName = "MyConsoleApp",
    // Timeout = 5000
}
```

#### `Url`

Base URL of your LogCenter server.

Example:

```csharp
Url = "http://localhost:9200"
```

#### `Table`

Destination table where logs will be stored in LogCenter.

Example:

```csharp
Table = "example_console"
```

#### `Token`

Access token generated in the LogCenter web interface.

#### `MinimumLevel`

Defines the minimum level that will be sent to LogCenter.

Example:

```csharp
MinimumLevel = LogLevel.Information
```

#### `ApplicationName`

Optional application name to include in the payload.

Example:

```csharp
ApplicationName = "MyConsoleApp"
```

#### `Timeout`

HTTP timeout in milliseconds for sending logs.

Example:

```csharp
Timeout = 5000
```

### Notes

- The package works with the normal `ILogger` API.
- Logs are sent over HTTP to LogCenter.
- Structured objects are serialized and stored in a JSON-friendly way.
- This package is a good fit for console apps, workers, background services, and other non-ASP.NET processes.

# LogCenter.RequestLogger

This lib will help you to send your app's request and responses from aspnet to LogCenter. See https://github.com/hudsonventura/LogCenter  

![Web Interface](https://github.com/hudsonventura/LogCenter/blob/main/logo.png?raw=true)



### Getting Started

You can use the LogCenter with aspnet in two different ways:
 - as a simple logger using the implementation of ILogger
 - as request and response logger  

Both are optional. You can choose one or both. Below you can see how to able them.

``` bash
dotnet add package LogCenter.RequestLogger
```

``` C#
using LogCenter;
using LogCenter.RequestInterceptor;

//Set your configs
InterceptorOptions options = new InterceptorOptions(){
    // LogCenter's URL
    Url = "http://localhost:9200",

    // Table name 
    Table = "example_interceptor",

    // Generate this on LogCenter inteface, on you profile photo.
    Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",

    // Log GET requests? Default is true
    //LogGetRequest = true,

    // TraceId will be sent on which header? OPTIONAL. Default is X-Trace-Id
    //TraceIdReponseHeader = "X-Trace-Id",

    //Some type of logs from your app by Microsoft or libs you dont have control to block. Here you can!
    BannedEventNames =
    {
        "ExecutingEndpoint",
        "Microsoft.EntityFrameworkCore.Database.Command.CommandExecutedExecuted"
    },
    BannedMessages =
    {
        "Request finished {Protocol} {Method} {Scheme}://{Host}{PathBase}{Path}{QueryString} - {StatusCode} {ContentLength} {ContentType} {ElapsedMilliseconds}ms",
    }
};



// Remove default logging (ILogger, console, debug, etc)
builder.Logging.ClearProviders();

// Add LogCenter provider, to use as ILogger in the controllers and other services.
builder.Logging.AddLogCenter(options);

...
var app = builder.Build();
...

// Use the interceptor to log request and response to LogCenter
app.UseRequestInterceptor();



//To use as a common logger here on Program.cs //OPTIONAL
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddLogCenter(options);
});
logger.LogInformation("Starting web application");
```

On your controller, you can use ILogger as usually:
``` C#
[ApiController]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> logger _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }


    [HttpGet(Name = "GetWeatherForecast")]
    public ActionResult Get()
    {
        _logger.LogInformation("Hello World - Info");
        _logger.LogWarning("Hello World - Warning");
        _logger.LogCritical("Hello World - Critical");
        _logger.LogDebug("Hello World - Debug");
        _logger.LogTrace("Hello World - Trace");
        _logger.LogError("Hello World - Error {@obj1} {@obj2}", test, new Exception("Test Exception"));


        return Ok();
    }
}
```

### About TraceId
Using this lib, it'ill add the responde header `TraceIdReponseHeader` (see above) in every single reponse. That is the same as aspnet TracerId, when a status code 400 is responded. It is saved on LogCenter. It can be used to localize the error that happend with your API clients.
```
TraceId: 00-4cda521494d8bf2337774936370e2cd3-3cec96b6ee169636-00

# OR

X-Trace-Id: 00-4cda521494d8bf2337774936370e2cd3-3cec96b6ee169636-00
```  


#### About LogCenterOptions.SaveFormatType
If `HTTPText`, the request and reponse is going to save like this example on the LogCenter. It's better to read, but **it doesn't work with jsonb search**.  

Request:
```
POST http://localhost:5000/WeatherForecast?QueryParam1=a_value&QueryParam2=test
Accept: */*
Connection: keep-alive
Host: localhost:5000
User-Agent: PostmanRuntime/7.43.0
Accept-Encoding: gzip, deflate, br
Cache-Control: no-cache
Content-Type: application/json
Content-Length: 57
MyHeader: Bearer mytoken
Postman-Token: 4467a9c1-2344-44c2-a579-f86cacbefa6e

{
    "sample": "it is an example",
    "value": 747
}
```


Response:
```
400 BadRequest
Sent to Address: 123.222.10.1
Content-Type: application/problem+json; charset=utf-8
Date: Mon, 24 Feb 2025 13:16:51 GMT
Server: Kestrel
Transfer-Encoding: chunked
traceId: 00-4cda521494d8bf2337774936370e2cd3-3cec96b6ee169636-00

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "$": [
      "The JSON value could not be converted to System.String. Path: $ | LineNumber: 0 | BytePositionInLine: 1."
    ],
    "body": [
      "The body field is required."
    ],
    "teste": [
      "The teste field is required."
    ]
  },
  "traceId": "00-4cda521494d8bf2337774936370e2cd3-3cec96b6ee169636-00"
}
```

If `Json` it'll save the request and response in json format, like this on LogCenter. It's not so good to read as a simples HTTPText, but **it works very well with jsonb search**.  

Request:
``` json
{
  "Body": {
    "value": 747,
    "sample": "it is an example"
  },
  "Host": "localhost:5000",
  "Path": "/WeatherForecast",
  "Type": "Request",
  "Query": {
    "QueryParam1": "a value",
    "QueryParam2": "test"
  },
  "Method": "POST",
  "Headers": {
    "Host": "localhost:5000",
    "Accept": "*/*",
    "MyHeader": "Bearer mytoken",
    "Connection": "keep-alive",
    "User-Agent": "PostmanRuntime/7.43.0",
    "Content-Type": "application/json",
    "Cache-Control": "no-cache",
    "Postman-Token": "e11bb8be-d92e-4e1d-91f2-e426b6de39b6",
    "Content-Length": "57",
    "Accept-Encoding": "gzip, deflate, br"
  },
  "CompleteURL": "http://localhost:5000/WeatherForecast?QueryParam1=a value_QueryParam2=test",
  "ReceivedFromAddress": "::1"
}
```

Response:
``` json
{
  "Body": {
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "errors": {
      "$": [
        "The JSON value could not be converted to System.String. Path: $ | LineNumber: 0 | BytePositionInLine: 1."
      ],
      "body": [
        "The body field is required."
      ],
      "teste": [
        "The teste field is required."
      ]
    },
    "status": 400,
    "traceId": "00-4973f8daf4b6be5b8fdbeba869d2ac1f-aeb071c7bf9432fc-00"
  },
  "Type": "Response",
  "Headers": {
    "Date": "Mon, 24 Feb 2025 13:18:10 GMT",
    "Server": "Kestrel",
    "traceId": "00-4973f8daf4b6be5b8fdbeba869d2ac1f-aeb071c7bf9432fc-00",
    "Content-Type": "application/problem+json; charset=utf-8",
    "Transfer-Encoding": "chunked"
  },
  "Exception": null,
  "StatusCode": 400,
  "ReasonPhrase": "BadRequest",
  "SentToAddress": "::1"
}
```



### Build

To build the NuGet package locally:

```bash
dotnet pack src/libs/nuget/RequestLogger/RequestLogger.csproj -c Release
```
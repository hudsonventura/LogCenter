# LogCenter.RequestLogger

This lib will help you to send your app's request and responses from aspnet to LogCenter. See https://github.com/hudsonventura/LogCenter  

![Web Interface](https://github.com/hudsonventura/LogCenter/blob/main/logo.png?raw=true)



### Getting Started



``` bash
dotnet add package LogCenter.RequestLogger
```

``` C#
using LogCenter;
using LogCenter.RequestInterceptor;

//Set your configs
InterceptorOptions options = new InterceptorOptions(){
    url = "http://localhost:9200",                                  // LogCenter's URL
    table = "example_interceptor",                                  // Table name 
    token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",              // Generate this on LogCenter inteface, on you profile photo.
    FormatType = InterceptorOptions.SaveFormatType.HTTPText,        // Save in HTTP Text or JSON?
    HideResponseExceptions = false,                                 // Hide Exceptions when 500 Internal server error (body) is returned to the user? Default is false, but it is recommended able it.   
    LogGetRequest = false,                                           // Log GET requests?
    TraceIdReponseHeader = "X-Trace-Id",                             // TraceId header name OPTIONAL. Default is X-Trace-Id
};

//inject the configs on dependency injection
builder.Services.AddSingleton<LogCenterOptions, InterceptorOptions>(op => options);

//inject the logger, to use on your controller
builder.Services.AddScoped<LogCenterLogger>();

...
var app = builder.Build();
...

//use the interceptor to log request and response
app.UseRequestInterceptor();
```

On your controller:
``` C#
[ApiController]
public class WeatherForecastController : ControllerBase
{
    private readonly LogCenter.LogCenterLogger _logger;

    public WeatherForecastController(LogCenter.LogCenterLogger logger)
    {
        _logger = logger;
    }


    [HttpGet(Name = "GetWeatherForecast")]
    public ActionResult Get()
    {
        _logger.Log(LogCenter.LogLevel.Debug, "Processing ...");
        _logger.Log(LogCenter.LogLevel.Debug, "The object", test);
        _logger.Log(LogCenter.LogLevel.Debug, "Ok ...");

        return Ok();
    }
}
```

### TraceId
Using this lib, it'ill add the responde header `TraceIdReponseHeader` (see above) in every single reponse. That is the same as aspnet TracerId, when a status code 400 is responded. It is saved on LogCenter. It can be used to localize the error that happend with your API clients.
```
TraceId: 00-4cda521494d8bf2337774936370e2cd3-3cec96b6ee169636-00

#OR

X-Trace-Id: 00-4cda521494d8bf2337774936370e2cd3-3cec96b6ee169636-00
```  


#### About LogCenterOptions.SaveFormatType
If `HTTPText`, it'll save something like this on the LogCenter. It's better to read, but **it doesn't work with jsonb search**.  

Request:
```
POST http://localhost:5000/WeatherForecast?QueryParam1=a value&QueryParam2=test
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
Sent to Address: ::1
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
  "CompleteURL": "http://localhost:5000/WeatherForecast?QueryParam1=a value&QueryParam2=test",
  "ReceivedFromAddress": "::1"
}
```

Request:
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
# LogCenter.RequestLogger

This lib will send your app's request and responses from FastAPI to LogCenter automatically. See https://github.com/hudsonventura/LogCenter

![Web Interface](https://github.com/hudsonventura/LogCenter/blob/main/logo.png?raw=true)




### Getting Started


### Basic usage
First install the lib from Pypi
``` bash
pip install pip install LogCenter.RequestLogger --no-cache-dir
```


Initialize the logger configs
``` python
from LogCenter import LogCenterOptions,LogCenterLogger, LogLevel

options = InterceptorOptions(
    url="http://localhost:9200",
    table="example_interceptor",                   # the spaces will be converted to _ (underscore).
    token="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9....", # Generate this on LogCenter inteface, on you profile photo.
    consoleLog = True,                                # Log the message on the console as a comon Console.WriteLine(). Default is true
    consoleLogEntireObject = True,                     # Log the entire objeti to the Console.WriteLine(). Default is false
    FormatType=SaveFormatType.Json,                     # Save in HTTP Text or JSON?
    LogGetRequest=True,                                 # Log GET requests?
    TraceIdReponseHeader="X-Trace-Id",                  # TraceId header name OPTIONAL. Default is X-Trace-Id
)

factory = LoggerFactory(options)
logger = factory.GetLogger()
get_logger = factory.GetLogger
app.add_middleware(InterceptorMiddleware, options=options, logger=logger)
```


Here the request and response will save to LogCenter.
``` python
@app.post("/echo")
async def echo(data: dict):
    return JSONResponse(content={"received": data})
```


Here, both the request and response will be saved to LogCenter, along with the message. All messages will have the same TraceID in LogCenter, making mapping and debugging easier.
``` python
@app.get("/")
async def root(logger=Depends(get_logger)):
    logger.LogAsync(LogLevel.INFO, "Hellooooo World =)")
    logger.LogAsync(LogLevel.FATAL, "Hellooooo World 321", [{"test": 123}])
    return {"message": "Hello, FastAPI!"}
```


Logging an object in the background without blocking code execution. You can use any type of object, which will be converted to JSON before being sent to LogCenter.
``` python
logger.LogAsync(LogLevel.CRITICAL, "This is just an example", {
    "nome": "John Doe Jr. da Silva",
    "idade": 30,
    "CPF": "123.456.789-00",
    "email": "john.doe.jr@example.com",
    "telefone": "11 91234-5678",
    "cep": "04545-000",
    "logradouro": "Rua Professor Atílio Innocenti, 165",
    "bairro": "Vila Nova Conceição",
    "cidade": "São Paulo",
    "uf": "SP",
    "pais": "Brasil",
    "dataDeNascimento": str(uuid.uuid4()),  # Simulação de data
    "altura": 1.80,
    "peso": 70.5,
    "descricao": "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat"
})
```


Logging an object in the foreground while blocking code execution. If you want to log in the current thread and wait for LogCenter's confirmation.  
Use this if it is the last log execution; otherwise, your app may terminate before your log is sent to LogCenter
``` python
logger.Log(LogLevel.ERROR, "Hello World")
```



## TraceID
A **Trace ID** (Tracing Identifier) is a unique identifier assigned to a request as it flows through a distributed system. It helps track the request across multiple services, making it easier to debug and monitor performance.

## 🔹 How It Works:
1. When a request enters a system (e.g., an API call), a **Trace ID** is generated.
2. This ID is **propagated** across all microservices and logs as the request moves through the system.
3. Developers can use it to **trace** the full lifecycle of a request across different services.


If you want to reference an ID (a Guid or a string) on LogCenter, you can use the TraceId. Let's see how to and cases:



``` python
/* In this case, you configure an ID (a GUID or a custom string) as a global Trace ID for the execution instance of your app.
 * Here, you are assigning the same Trace ID to all logs for that instance's execution.
 */
logger = LogCenterLogger(options, 
                         #trace_id = str(uuid.uuid4()) # It's optional. If empty, it will generate a new one Guid, else, you can you your own traceId, Guid or string
                         )

// AND / OR

/* In this case, you configure an ID (a GUID or a custom string) for each log entry. It will ignore the global Trace ID (above).
 * Here, you are assigning a Trace ID to each execution. It is useful for multiple thread executions in your app.
 * You can generate a Trace ID at the beginning of each thread and pass it to the Log method call.
 */
traceId = str(uuid.uuid4()
logger.LogAsync(LogLevel.Info, "Hello World - Info", traceId)

//OR

/* Same as above, but with a personal/custom string
 */
myTrace = "My custom trace execution ID"
logger.LogAsync(LogLevel.Info, "Hello World - Info", myTrace)
```



#### Build Package

On `pyproject.toml` file directory:
```bash
python -m build # must be installed before -> pip install build
twine upload dist/* # must be installed before -> pip install twine
pip install LogCenter --no-cache-dir
```
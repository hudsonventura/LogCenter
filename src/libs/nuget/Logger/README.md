# LogCenter.Logger

This lib will help you to send your app's logs to LogCenter. See https://github.com/hudsonventura/LogCenter  

<p align="center">
  <img src="https://github.com/hudsonventura/LogCenter/blob/main/logo.png?raw=true" alt="Descrição da imagem" width="230px">
</p>



### Getting Started


### Basic usage
First install the lib from Nuget
``` bash
dotnet add package LogCenter.Logger
```


Initialize the logger with you LogCenter configs
``` C#
using LogCenter;

LogCenterLogger logger = new LogCenterLogger(new LogCenterOptions(){
    url = "http://localhost:9200",
    table = "example dotnet console",                   //the spaces will be converted to _ (underscore).
    token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9....", // Generate this on LogCenter inteface, on you profile photo.
    consoleLog = true,                                  // Log the message on the console as a comon Console.WriteLine(). Default is true
    consoleLogEntireObject = true                       // Log the entire objeti to the Console.WriteLine(). Default is false
});
```

Logging a simple message in the background without blocking code execution. It'll start a new thread to send the log to LogCenter.
``` C#
// Use this 
logger.Log(LogLevel.Trace, "Hello World");
```


Logging an object in the background without blocking code execution. You can use any type of object, which will be converted to JSON before being sent to LogCenter.
``` C#
string message = "Hello World ";
object anObject = new { 
    name = "John Doe",
    age = 30,
    doc = "123.456.789-00",
    email = "john.doe@example.com",
    fone = "11 91234-5678"
};
logger.Log(LogLevel.Trace, message, anObject);
```


Logging an object in the foreground while blocking code execution. If you want to log in the current thread and wait for LogCenter's confirmation.  
Use this if it is the last log execution; otherwise, your app may terminate before your log is sent to LogCenter
``` C#
await logger.LogAsync(LogLevel.Critical, "Hello World 2", new { 
    name = "John Doe",
    age = 30,
    doc = "123.456.789-00",
    email = "john.doe@example.com",
    fone = "11 91234-5678"
});
```




## TraceID
A **Trace ID** (Tracing Identifier) is a unique identifier assigned to a request as it flows through a distributed system. It helps track the request across multiple services, making it easier to debug and monitor performance.

## 🔹 How It Works:
1. When a request enters a system (e.g., an API call), a **Trace ID** is generated.
2. This ID is **propagated** across all microservices and logs as the request moves through the system.
3. Developers can use it to **trace** the full lifecycle of a request across different services.


If you want to reference an ID (a Guid or a string) on LogCenter, you can use the TraceId. Let's see how to and cases:



``` C#
/* In this case, you configure an ID (a GUID or a custom string) as a global Trace ID for the execution instance of your app.
 * Here, you are assigning the same Trace ID to all logs for that instance's execution.
 */
LogCenterLogger logger = new LogCenterLogger(
    new LogCenterOptions(){
        ...
    } 
    , Guid.NewGuid() //or a string. It's optional. If empty, it will generate a new one Guid, else, you can you your own traceId, Guid or string
);

// AND / OR

/* In this case, you configure an ID (a GUID or a custom string) for each log entry. It will ignore the global Trace ID (above).
 * Here, you are assigning a Trace ID to each execution. It is useful for multiple thread executions in your app.
 * You can generate a Trace ID at the beginning of each thread and pass it to the Log method call.
 */
Guid traceId = Guid.NewGuid();
logger.Log(LogLevel.Info, "Hello World - Info", traceId);

//OR

/* Same as above, but with a personal/custom string
 */
string myTrace = "My custom trace execution ID";
logger.Log(LogLevel.Info, "Hello World - Info", myTrace);
```
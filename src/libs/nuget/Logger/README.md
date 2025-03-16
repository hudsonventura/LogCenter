# LogCenter.Logger

This lib will help you to send your app's logs to LogCenter.  

<p align="center">
  <img src="https://github.com/hudsonventura/LogCenter/blob/main/logo.png?raw=true" alt="Descrição da imagem" width="230px">
</p>

Each log entry is queued and will be sent in order, following a FIFO (First In, First Out) approach.  
This ensures that logs are processed sequentially, maintaining the correct logging sequence.  


### Getting Started


### Basic usage
First install the lib from Nuget
``` bash
dotnet add package LogCenter.Logger --version 1.*
```


Initialize the logger with you LogCenter configs
``` C#
using LogCenter;

LogCenterLogger logger = new LogCenterLogger(new LogCenterOptions(){
    url = "http://localhost:9200",
    table = "example dotnet console",                   //the spaces will be converted to _ (underscore).
    token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9....", // Generate this on LogCenter inteface, on you profile photo.
    consoleLog = true,                                   // Log the message on the console as a comon Console.WriteLine(). Default is true
    consoleLogEntireObject = true                        // Log the entire objeti to the Console.WriteLine(). Default is false
});
```

Logging a simple message in the background without blocking code execution
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
If you want to reference an ID (a Guid or a string), you can use the TraceId. Let's see:

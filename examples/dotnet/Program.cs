// See https://aka.ms/new-console-template for more information
using LogCenter;


LogCenterLogger logger = new LogCenterLogger(new LogCenterOptions(){
    url = "http://localhost:9200",
    table = "example dotnet console",
    token = "token",
});





logger.Log("Hello World 2", LogLevel.Trace);


//prevent.Stop();


Console.WriteLine("Cabô");

await logger.LogAsync(123);


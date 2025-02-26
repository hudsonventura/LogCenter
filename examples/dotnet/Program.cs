// See https://aka.ms/new-console-template for more information
using LogCenter;


LogCenterLogger logger = new LogCenterLogger(new LogCenterOptions(){
    url = "http://localhost:9200",
    table = "example dotnet console",
    token = "token",
});





logger.Log(LogLevel.Trace, "Hello World 1");


//prevent.Stop();


Console.WriteLine("Cabô");

await logger.LogAsync(LogLevel.Critical, "Hello World 2", new { 
    nome = "John Doe",
    idade = 30,
    cpf = "123.456.789-00",
    email = "john.doe@example.com",
    telefone = "11 91234-5678"
});


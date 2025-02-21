using Serilog;
using nuget;

var builder = WebApplication.CreateBuilder(args);


////////////////
// Configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    //.WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    //.Enrich.FromLogContext()
    // .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200/teste123"))
    //         {
    //             AutoRegisterTemplate = true, // Registra automaticamente o template no Elasticsearch
    //             IndexFormat = "meu-app-logs", // Nome do índice no formato desejado
    //             FailureCallback = (logEvent, exception) =>
    //                 Console.WriteLine($"Erro ao enviar log: {exception?.Message} | Evento: {logEvent?.RenderMessage()}"),
    //             EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog
    //         })
    // .WriteTo.LogCenter(new LogCenterSinkOptions(){
    //     url = "http://localhost:9200",
    //     table = "example",
    //     //timezone = "timezone", //Default is UTC
    // })
    .CreateLogger();

// Adiciona o Serilog ao ASP.NET
//builder.Host.UseSerilog();
//Log.Information("LogCenter test message");
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new LogCenterProvider(new LogCenterSinkOptions(){
        url = "http://localhost:9200",
        table = "example",
        //timezone = "timezone", //Default is UTC
    }));
  // Adiciona logs no Event Viewer (Windows)

///////////////
///


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

using Serilog;
using nuget;

var builder = WebApplication.CreateBuilder(args);


////////////////
// Configuração
builder.Services.AddScoped(provider =>
{
    nuget.ILogger logger = new nuget.LogCenterLogger(new LogCenterOptions(){
        url = "http://localhost:9200",
        table = "example",
        //timezone = "timezone", //Default is UTC
    });
    return logger;
});
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

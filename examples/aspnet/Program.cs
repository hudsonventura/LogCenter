using Serilog;
using LogCenter; //TODO: DOCS
using LogCenter.RequestInterceptor; //TODO: DOCS

var builder = WebApplication.CreateBuilder(args);


////////////////
// Configuração logger //TODO: DOCS
builder.Services.AddScoped(provider =>
{
    LogCenter.ILogger logger = new LogCenterLogger(new LogCenterOptions(){
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

//TODO: DOCS

app.UseInterceptor(new LogCenterOptions(){
    url = "http://localhost:9200",
    table = "example_interceptor",
    formatType = LogCenterOptions.SaveFormatType.HTTPText,
    LogGetRequest = false,
    hideResponseExceptions = false
});



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

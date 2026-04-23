using LogCenter;
using LogCenter.RequestInterceptor;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure LogCenter e o interceptor HTTP usando a mesma configuração base.
var logCenterOptions = new InterceptorOptions
{
    Url = "http://localhost:9200",
    Table = "example_console",
    Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AYWRtaW4uY29tIiwibmFtZSI6ImV4YW1wbGVfY29uc29sZSIsInRhYmxlcyI6ImV4YW1wbGVfY29uc29sZSIsImV4cCI6MTc3OTQwMTg5NSwiaXNzIjoiU2V1SXNzdWVyIiwiYXVkIjoiU2V1QXVkaWVuY2UifQ.SOCfxUeUOhOJ3etnYXn71BoDv03C6xXPKXA0ocPqtRU",
    FormatType = InterceptorOptions.SaveFormatType.Json,
    LogGetRequest = true
};

builder.Services.AddSingleton(logCenterOptions);
builder.Services.AddSingleton<LogCenterOptions>(logCenterOptions);

// Remove todos os loggers padrão (console, debug, etc)
builder.Logging.ClearProviders();

// Adiciona apenas o LogCenter
builder.Logging.AddLogCenter(logCenterOptions);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestInterceptor();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

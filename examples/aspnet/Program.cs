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
    Table = "example_aspnet",
    Token = "seu_token_aqui",
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

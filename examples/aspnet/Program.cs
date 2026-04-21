using LogCenter;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure LogCenter - apenas informe url, table e token
var logCenterOptions = new LogCenterOptions
{
    url = "http://localhost:9200",
    table = "example_aspnet",
    token = "seu_token_aqui"
};

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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

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
    Token = "eyJhbGciOiJIUzI1NiIsI...",
    LogGetRequest = true,
    BannedEventNames =
    {
        "ExecutingEndpoint",
        "ControllerActionExecuting",
        "ActionExecuted",
        "ExecutingEndpointExecuting",
        "ExecutedEndpoint",
        "ObjectResultExecuting",
        "PolicySuccess",
        "Microsoft.EntityFrameworkCore.Database.Command.CommandExecutedExecuted"
    },
    BannedMessages =
    {
        "Request finished {Protocol} {Method} {Scheme}://{Host}{PathBase}{Path}{QueryString} - {StatusCode} {ContentLength} {ContentType} {ElapsedMilliseconds}ms",
        "Request starting {Protocol} {Method} {Scheme}://{Host}{PathBase}{Path}{QueryString} - {ContentType} {ContentLength}"
    }
};


builder.Services.AddSingleton<LogCenterOptions>(logCenterOptions);

// Remove default logging (console, debug, etc)
builder.Logging.ClearProviders();

// Add only LogCenter provider, to use as ILogger in the controllers and other services. It will send logs to the configured LogCenter URL.
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

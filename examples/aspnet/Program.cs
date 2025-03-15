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


app.UseRequestInterceptor(new InterceptorOptions(){
    url = "http://localhost:9200",                                  // LogCenter's URL
    table = "example_interceptor",                                  // Table name 
    FormatType = InterceptorOptions.SaveFormatType.HTTPText,        // Save in HTTP Text or JSON?
    HideResponseExceptions = false,                                 // Hide Exceptions when 500 Internal server error is returned to the user?    
    LogGetRequest = false                                           // Log GET requests?
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

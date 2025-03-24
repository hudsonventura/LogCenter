using LogCenter;
using LogCenter.RequestInterceptor;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");



builder.Services.AddScoped(provider =>
{
    LogCenter.ILogger logger = new LogCenterLogger(new LogCenterOptions(){
        url = "http://localhost:9200",
        table = "example",
    });
    return logger;
});



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseRequestInterceptor(new InterceptorOptions(){
    url = "http://localhost:9200",                                  // LogCenter's URL
    table = "example_interceptor",                                  // Table name 
    token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AYWRtaW4uY29tIiwibmFtZSI6ImV4YW1wbGVfaW50ZXJjZXB0b3IiLCJ0YWJsZXMiOiJleGFtcGxlX2ludGVyY2VwdG9yIiwiZXhwIjoyMDU3NTI4NjM5LCJpc3MiOiJTZXVJc3N1ZXIiLCJhdWQiOiJTZXVBdWRpZW5jZSJ9.h7HW5m4FIxM7tanZmjWxCMHIfUSz-1MVFYOCv1k69UI",
    FormatType = InterceptorOptions.SaveFormatType.HTTPText,        // Save in HTTP Text or JSON?
    HideResponseExceptions = false,                                 // Hide Exceptions when 500 Internal server error is returned to the user?    
    LogGetRequest = false,                                           // Log GET requests?
    TraceIdReponseHeader = "X-Trace-Id",                             // TraceId header name OPTIONAL. Default is X-Trace-Id
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

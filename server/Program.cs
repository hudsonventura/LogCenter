using Microsoft.OpenApi.Models;
using System.Reflection;

using DotNetEnv;
using Npgsql;
using server.Repositories;
using server.BackgroundServices;




var builder = WebApplication.CreateBuilder(args);
Env.Load();


string dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? string.Empty;
string dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? string.Empty;
string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? string.Empty;
string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? string.Empty;
string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? string.Empty;

string connectionString = $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPassword};Database={dbName}";
Console.WriteLine($"connectionString -> {connectionString}");

if(dbHost == string.Empty){
    Console.WriteLine("The environment variables is null or not present. Check the .env file.");
    Environment.Exit(1);  // 0 indica saída bem-sucedida
}




builder.Services.AddTransient<NpgsqlConnection>(sp =>
{
    return new NpgsqlConnection(connectionString);
});

builder.Services.AddScoped<DBRepository>();
builder.Services.AddHostedService<RecyclingRecords>();


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LogCenter", Version = "1.0" });

    // Inclua o caminho para o arquivo XML gerado
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) //use swagger in dev
{
    app.UseSwagger();
    app.UseSwaggerUI();
}else{ //use redoc in PROD
    {
    app.UseSwagger();
    app.UseReDoc(c =>
    {
        c.DocumentTitle = "LogCenter";
        c.SpecUrl = "/swagger/v1/swagger.json";
    });
}
}

app.UseHttpsRedirection();


app.MapControllers();

app.Run();


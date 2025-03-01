using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

using DotNetEnv;
using Npgsql;
using server.Repositories;
using server.BackgroundServices;
using server.Domain;

AppContext.SetSwitch("System.Globalization.Invariant", true);
TimeZoneInfo utcZone = TimeZoneInfo.CreateCustomTimeZone("UTC", TimeSpan.Zero, "UTC", "UTC");
TimeZoneInfo.ClearCachedData();
TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, utcZone);




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

//Apply DB migration
using (DBRepository db = new DBRepository(new NpgsqlConnection(connectionString)))
{
    db.CreateExtensions();
    db.CreateConfigTable();
}


builder.Services.AddTransient<NpgsqlConnection>(sp =>
{
    return new NpgsqlConnection(connectionString);
});

builder.Services.AddScoped<DBRepository>();
builder.Services.AddHostedService<RecyclingRecords>();

builder.Services.AddSingleton<LastRecordIDRepository>();

//Context for users, token and authorization
builder.Services.AddDbContext<UserContext>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LogCenter", Version = "1.0" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Services.AddControllers();

string listen = Environment.GetEnvironmentVariable("ASPNETCORE_LISTEN") ?? "http://localhost:9200";
builder.WebHost.UseUrls(listen);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) //use swagger in dev
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Definindo o prefixo swagger para evitar conflitos
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LogCenter V1");
        c.RoutePrefix = "docs/swagger"; // Define o prefixo da rota do Swagger
    });
}else{
    app.UseSwagger();

    app.UseReDoc(c =>
    {
        c.DocumentTitle = "LogCenter";
        c.SpecUrl = "/swagger/v1/swagger.json";
        c.RoutePrefix = "docs/swagger/redoc"; // Define o prefixo para o ReDoc
    });
}

app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
});

//se o banco não existir, cria
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserContext>();
    dbContext.Database.Migrate();

    var adminUser = dbContext.Users.FirstOrDefault(u => u.email == "admin@logcenter.org");
    if (adminUser == null)
    {
        var admin = new User
        {
            name = "admin",
            email = "admin@logcenter.org",
        };
        admin.SetPassword("admin");
        dbContext.Users.Add(admin);
        dbContext.SaveChanges();
    }
}


app.UseHttpsRedirection();


app.MapControllers();

app.Run();

public partial class Program { } //for tests
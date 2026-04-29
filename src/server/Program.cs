using Microsoft.EntityFrameworkCore;

using DotNetEnv;
using Npgsql;
using server.Repositories;
using server.BackgroundServices;
using server.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;

AppContext.SetSwitch("System.Globalization.Invariant", true);
TimeZoneInfo utcZone = TimeZoneInfo.CreateCustomTimeZone("UTC", TimeSpan.Zero, "UTC", "UTC");
TimeZoneInfo.ClearCachedData();
TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, utcZone);




var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? Environments.Production
});
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
    db.CreateMaintenanceLogTable();
}


builder.Services.AddTransient<NpgsqlConnection>(sp =>
{
    return new NpgsqlConnection(connectionString);
});

builder.Services.AddScoped<DBRepository>();
builder.Services.AddHostedService<RecyclingRecords>();

builder.Services.AddSingleton<LastRecordIDRepository>();
builder.Services.AddSingleton<TokenRepository>();

//Context for users, token and authorization
builder.Services.AddDbContext<UserContext>();

// Add services to the container.
builder.Services.AddOpenApi("v1");
builder.Services.AddControllers();

string listen = Environment.GetEnvironmentVariable("ASPNETCORE_LISTEN") ?? "http://0.0.0.0:9200";
builder.WebHost.UseUrls(listen);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string secretJWTKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new Exception("JWT_KEY not found");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            //ValidateIssuer = true,
            //ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "SeuIssuer",
            ValidAudience = "SeuAudience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretJWTKey))
        };
		options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                // Impede que a resposta padrão do OnChallenge seja executada
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var errorResponse = new
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Bearer token is missing or invalid. Check your header Authorization or generate a new token",
                    Timestamp = DateTime.UtcNow
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            },
            OnAuthenticationFailed = async context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    //context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var errorResponse = new
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = "Token de autenticação ausente ou inválido.",
                        Timestamp = DateTime.UtcNow
                    };

                    //await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                    await Task.CompletedTask;
                }
            },
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();

app.MapOpenApi("/openapi/{documentName}.json");
app.MapScalarApiReference(options =>
{
    options.Title = "Docs LogCenter API";
    options.ShowSidebar = true;
    options.WithTitle("LogCenter Docs");
    options.Favicon = "https://github.com/hudsonventura/LogCenter/blob/main/logo.png?raw=true";
});



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

    var adminUser = dbContext.Users.FirstOrDefault(u => u.email == "admin@admin.com");
    if (adminUser == null)
    {
        var admin = new User
        {
            name = "admin",
            email = "admin@admin.com",
        };
        admin.SetPassword("admin");
        dbContext.Users.Add(admin);
        dbContext.SaveChanges();
    }
}


app.UseHttpsRedirection();

// Ativar autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();

public partial class Program { } //for tests

using Microsoft.OpenApi.Models;
using System.Reflection;
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
using Scalar.AspNetCore;

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
builder.Services.AddSingleton<TokenRepository>();

//Context for users, token and authorization
builder.Services.AddDbContext<UserContext>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddOpenApi(); //net8.0 or higher
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LogCenter Docs",
        Version = "1.0",
        Description = @"LogCenter API - Log Store and Trace<br>
        <img src='https://github.com/hudsonventura/LogCenter/blob/main/logo.png?raw=true' alt='LogCenter Logo' width='230px'>
        <p>
            Repository: <a href='https://github.com/hudsonventura/LogCenter'>https://github.com/hudsonventura/LogCenter</a>
        </p>
        <p>
            Thanks for <a href='https://github.com/LucasLuann'>https://github.com/LucasLuann</a>
        </p>",
        Contact = new OpenApiContact
        {
            Name = "LogCenter",
            Url = new Uri("https://github.com/hudsonventura/LogCenter")
        }
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Services.AddControllers();

string listen = Environment.GetEnvironmentVariable("ASPNETCORE_LISTEN") ?? "http://localhost:9200";
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

//Use Scalar API Docs
app.UseSwagger(options =>
{
    options.RouteTemplate = "/openapi/{documentName}.json";
});
app.MapScalarApiReference(options =>
{

    // Object initializer
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

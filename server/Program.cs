using DotNetEnv;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
Env.Load();


string dbHost = Environment.GetEnvironmentVariable("DB_HOST");
string dbUser = Environment.GetEnvironmentVariable("DB_USER");
string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
string dbName = Environment.GetEnvironmentVariable("DB_NAME");

string connectionString = $"Host={dbHost};Username={dbUser};Password={dbPassword};Database={dbName}";
//Console.WriteLine($"connectionString -> {connectionString}");


builder.Services.AddTransient<NpgsqlConnection>(sp =>
{
    return new NpgsqlConnection(connectionString);
});


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.Run();


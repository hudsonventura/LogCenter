using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using server.Domain;

namespace server.Repositories;

public class UserContext : DbContext
{
    private string stringConnection = $@"NOT SET YET";
    public UserContext(IConfiguration appsettings) : base()
    {
        string host = Environment.GetEnvironmentVariable("DB_HOST") ?? string.Empty;
        string port = Environment.GetEnvironmentVariable("DB_PORT") ?? string.Empty;
        string username = Environment.GetEnvironmentVariable("DB_USER") ?? string.Empty;
        string password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? string.Empty;
        string database = Environment.GetEnvironmentVariable("DB_NAME") ?? string.Empty;

        stringConnection = $"Host={host};Port={port};Username={username};Password={password};Database=Users";
        Console.WriteLine($"connectionString -> {stringConnection}");
    }
 
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(stringConnection);
  
 
 
 
     public DbSet<User> Users { get; set; }
}
/*
    var options = new DbContextOptionsBuilder<ReservaSalasContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;
        _dbContext = new ReservaSalasContext(options);
*/
using DotNetEnv;
using Npgsql;
using Bogus;
using Newtonsoft.Json;
using server.Repositories;
using server.Domain;


string dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? string.Empty;
string dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? string.Empty;
string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? string.Empty;
string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? string.Empty;
string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? string.Empty;

string connectionString = $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPassword};Database={dbName}";
Console.WriteLine($"connectionString -> {connectionString}");

string table = "SEAPI";

if(dbHost == string.Empty){
    Console.WriteLine("The environment variables is null or not present. Check the .env file.");
    Environment.Exit(1);  // 0 indica saída bem-sucedida
}

var conn = new NpgsqlConnection(connectionString);
DBRepository _db = new DBRepository(conn);

int contador = 0;
Random random = new Random();

//loop para criar os registros
for (int i = 0; i < 1_000_000; i++){
    var faker = new Faker<MyObjectExample>()
        .RuleFor(o => o.Name, f => f.Person.FullName)
        .RuleFor(o => o.Email, f => f.Person.Email)
        .RuleFor(o => o.Age, f => f.Random.Int(18, 65));

    // Gerando o objeto
    var objetoFicticio = faker.Generate();

    string gerado = JsonConvert.SerializeObject(objetoFicticio);

    // Gera um texto Lorem Ipsum
    //var faker = new Faker();
    //string gerado = faker.Lorem.Paragraphs(1); // Gera 1 parágrafo


    int number = random.Next(1, 6);
    Level level = (Level)number;


    _db.Insert(table, level, gerado);

    
    // Opcional: Log para cada 100.000 inserções
    if (contador % 100 == 0)
    {
        Console.WriteLine($"{contador} registros gerados ...");
    }
    contador++;
}






Console.WriteLine("Inserção concluída.");


class MyObjectExample{
    public int Id { get; set;} = 0;
    public string Name { get; set;}
    public string Email { get; set;}
    public int Age { get; set;}
}
using DotNetEnv;
using Npgsql;
using Bogus;
using Newtonsoft.Json; 


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


// Gera um texto aleatório usando Bogus (Lorem Ipsum)
        
        
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            
            // Usamos um comando de INSERT com parâmetros
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "INSERT INTO teste (content) VALUES (to_jsonb(@value));";

                
                cmd.Parameters.Add(new NpgsqlParameter("@value", NpgsqlTypes.NpgsqlDbType.Text));

                // Loop para inserir 1 milhão de registros
                for (int i = 0; i < 1_000_000; i++)
                {

                    // Definindo o objeto com o Bogus
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

                    // Define o valor do parâmetro
                    cmd.Parameters["@value"].Value = gerado;

                    // Executa o INSERT
                    cmd.ExecuteNonQuery();

                    // Opcional: Log para cada 100.000 inserções
                    if (i % 100 == 0)
                    {
                        Console.WriteLine($"{i} registros inseridos...");
                    }
                }

                Console.WriteLine("Inserção concluída.");
            }
        }

class MyObjectExample{
    public int Id { get; set;} = 0;
    public string Name { get; set;}
    public string Email { get; set;}
    public int Age { get; set;}
}
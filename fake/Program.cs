using DotNetEnv;
using Npgsql;
using Bogus;


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
        var faker = new Faker();
        
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            
            // Usamos um comando de INSERT com parâmetros
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "INSERT INTO teste (content) VALUES (@texto)";

                
                cmd.Parameters.Add(new NpgsqlParameter("@texto", NpgsqlTypes.NpgsqlDbType.Text));

                // Loop para inserir 1 milhão de registros
                for (int i = 0; i < 1_000_000; i++)
                {
                    // Gera um texto Lorem Ipsum
                    string textoAleatorio = faker.Lorem.Paragraphs(1); // Gera 1 parágrafo

                    // Define o valor do parâmetro
                    cmd.Parameters["@texto"].Value = textoAleatorio;

                    // Executa o INSERT
                    cmd.ExecuteNonQuery();

                    // Opcional: Log para cada 100.000 inserções
                    if (i % 100_000 == 0)
                    {
                        Console.WriteLine($"{i} registros inseridos...");
                    }
                }

                Console.WriteLine("Inserção concluída.");
            }
        }
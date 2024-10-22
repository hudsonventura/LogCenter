using System.Text.RegularExpressions;
using Npgsql;
using server.Domain;
using SnowflakeID;

namespace server.Repositories;

public class DBRepository : IDisposable
{
    public DBRepository(NpgsqlConnection connection)
    {
        _conn = connection;
        _conn.Open();
    }

    NpgsqlConnection _conn;
    private TimeSpan _tz = TimeSpan.Zero;

    public void Dispose()
    {
        if (_conn.State == System.Data.ConnectionState.Open)
        {
            _conn.Close();
        }
    }

    /// <summary>
    /// Creates the necessary extensions 
    /// </summary>
    internal void CreateExtensions(){
        using var command = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS pg_trgm", _conn);
        command.ExecuteNonQuery();
    }
    

    /// <summary>
    /// List tables for frontend use
    /// </summary>
    /// <returns></returns>
    public List<string> ListTabels()
    {
        var tables = new List<string>();

        using var command = new NpgsqlCommand(@"
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
            AND table_type = 'BASE TABLE';", _conn);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            // Adiciona o nome da tabela à lista
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    public void CreateTable(string index){
        string txt_command = @$"CREATE TABLE {index} (
                                id BIGINT PRIMARY KEY,
                                level SMALLINT CHECK (level >= 0 AND level <= 9),
                                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                description VARCHAR(255) NULL,
                                content jsonb
                            );";
        using var command = new NpgsqlCommand(txt_command, _conn);
        command.ExecuteNonQuery();
    }

    public void CreateDescriptionbIndex(string index){
        string txt_command = 
            @$"CREATE INDEX idx_{index}_desc ON {index} USING GIN (description gin_trgm_ops);";
        using var command = new NpgsqlCommand(txt_command, _conn);
        command.ExecuteNonQuery();
    }

    public void DropTable(string index)
    {
        string txt_command = $"DROP TABLE IF EXISTS {index};";
        using var command = new NpgsqlCommand(txt_command, _conn);
        command.ExecuteNonQuery();
    }




    public void CreateJsonbIndex(string index){
        string txt_command = 
            @$"CREATE INDEX idx_{index} ON {index} USING GIN (content jsonb_ops);";
        using var command = new NpgsqlCommand(txt_command, _conn);
        command.ExecuteNonQuery();
    }

    //Not necessary
    // public void DeleteJsonbIndex(string index)
    // {
    //     string txt_command = $"DROP INDEX IF EXISTS idx_{index};";
    //     using var command = new NpgsqlCommand(txt_command, _conn);
    //     command.ExecuteNonQuery();
    // }




    public void ValidateTable(string table)
    {
        var regex = new Regex(@"[!@#$%^&*(),.?""{}|<>]");
        if(regex.IsMatch(table)){
            throw new Exception("The index must be a string without special chars");
        }

    }

    public ulong Insert(string table, Level level, string description, string json)
    {
        // Gera o ID Snowflake
        ulong id = SnowflakeIDGenerator.GetSnowflake(0).ToUInt64();

        // Cria o comando de inserção
        using var command = new NpgsqlCommand($"INSERT INTO {table} (id, level, description, content) VALUES (@id, @level, @description, @value::jsonb)", _conn);

        // Define o parâmetro 'id' explicitamente como BIGINT
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Bigint) { Value = (long)id });

        command.Parameters.Add(new NpgsqlParameter("level", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (int)level });

        command.Parameters.Add(new NpgsqlParameter("description", NpgsqlTypes.NpgsqlDbType.Text) { Value = description ?? (object)DBNull.Value });

        // Adiciona o parâmetro 'value' com o JSON
        command.Parameters.AddWithValue("value", NpgsqlTypes.NpgsqlDbType.Jsonb, json);

        // Executa o comando
        command.ExecuteNonQuery();

        // Retorna o ID gerado
        return id;
    }

    internal List<Record> Search(string table, SearchObject query)
    {
    // Cria o comando de consulta com um intervalo de datas
    using var command = new NpgsqlCommand(@$"
        SELECT id, level, created_at, description, content 
        FROM {table} 
        WHERE 1=1 
        and created_at BETWEEN @datetime1 AND @datetime2
        and content::text ILIKE @search
        --
        ORDER BY id
        LIMIT @take -- Toma 20 registros
        OFFSET @skip
        ", _conn);

    // Define os parâmetros para datetime1 e datetime2
        DateTime datetime1 = (query.datetime1 > DateTime.MinValue) ? DateTime.SpecifyKind(query.datetime1, DateTimeKind.Utc) : DateTime.UtcNow.AddHours(-1);
        DateTime datetime2 = (query.datetime2 > DateTime.MinValue) ? DateTime.SpecifyKind(query.datetime2, DateTimeKind.Utc) : DateTime.UtcNow;
        command.Parameters.Add(new NpgsqlParameter("datetime1", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = datetime1.Add(-_tz) });
        command.Parameters.Add(new NpgsqlParameter("datetime2", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = datetime2.Add(-_tz) });

    command.Parameters.Add(new NpgsqlParameter("take", NpgsqlTypes.NpgsqlDbType.Integer) { Value = query.take });
    command.Parameters.Add(new NpgsqlParameter("skip", NpgsqlTypes.NpgsqlDbType.Integer) { Value = query.skip });

    command.Parameters.Add(new NpgsqlParameter("search", NpgsqlTypes.NpgsqlDbType.Text) { Value = $"%{query.search}%" });


    // Executa o comando e obtém o resultado
    using var reader = command.ExecuteReader();

    // Cria uma lista para armazenar os resultados
    var results = new List<Record>();

    // Percorre todos os registros retornados
    while (reader.Read())
    {
        // Cria um objeto dinâmico para armazenar os valores da linha
        var record = new Record
        {
            id = reader.GetInt64(reader.GetOrdinal("id")),
            level = (Level)reader.GetInt64(reader.GetOrdinal("level")),
            description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
            created_at = reader.GetDateTime(reader.GetOrdinal("created_at")).Add(_tz),
            content = System.Text.Json.JsonSerializer.Deserialize<dynamic>(reader["content"].ToString())
        };

        // Adiciona o objeto dinâmico à lista de resultados
        results.Add(record);
    }

    // Retorna a lista de resultados
    return results.OrderByDescending(x => x.id).ToList();
}


    public void DeleteRecords(string table, DateTime before_date)
    {
        Console.WriteLine($"It will remove records before {before_date} from table {table}");
        // Prepara o comando de deletar
        using var command = new NpgsqlCommand(@$"
            DELETE FROM {table} 
            WHERE created_at < @before_date;", _conn);

        // Define o parâmetro 'cutdate' como 'TIMESTAMP WITH TIME ZONE'
        command.Parameters.Add(new NpgsqlParameter("before_date", NpgsqlTypes.NpgsqlDbType.TimestampTz)
        {
            Value = DateTime.SpecifyKind(before_date, DateTimeKind.Utc) // Certifique-se que a data é UTC
        });


        int effected = command.ExecuteNonQuery();
        Console.WriteLine($"Removed {effected} records from table {table}");
    }

    internal Record GetByID(string table, long id)
    {
        using var command = new NpgsqlCommand(@$"
            SELECT id, level, created_at, description, content 
            FROM {table} 
            WHERE 1=1 
            and id = @id", _conn);

        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Bigint) { Value = id });


        // Executa o comando e obtém o resultado
        using var reader = command.ExecuteReader();

        // Percorre todos os registros retornados
        while (reader.Read())
        {
            // Cria um objeto dinâmico para armazenar os valores da linha
            var record = new Record
            {
                id = reader.GetInt64(reader.GetOrdinal("id")),
                level = (Level)reader.GetInt64(reader.GetOrdinal("level")),
                description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                created_at = reader.GetDateTime(reader.GetOrdinal("created_at")).Add(_tz),
                content = System.Text.Json.JsonSerializer.Deserialize<dynamic>(reader["content"].ToString())
            };

            // Adiciona o objeto dinâmico à lista de resultados
            return record;
        }

        // Retorna a lista de resultados
        return null;
    }


    public void SetTimezone(int timezone){
        _tz = TimeSpan.FromHours(timezone);
    }

    
}

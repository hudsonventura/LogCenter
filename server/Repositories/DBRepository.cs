using System.Text.RegularExpressions;
using Npgsql;
using server.Domain;
using SnowflakeID;

namespace server.Repositories;

public class DBRepository
{
    public DBRepository(NpgsqlConnection connection)
    {
        _conn = connection;
        _conn.Open();
    }

    NpgsqlConnection _conn;

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
                                content jsonb
                            );";
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

    public ulong Insert(string table, Level level, string json)
    {
        // Gera o ID Snowflake
        ulong id = SnowflakeIDGenerator.GetSnowflake(0).ToUInt64();

        // Cria o comando de inserção
        using var command = new NpgsqlCommand($"INSERT INTO {table} (id, level, content) VALUES (@id, @level, @value::jsonb)", _conn);

        // Define o parâmetro 'id' explicitamente como BIGINT
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Bigint) { Value = (long)id });

        command.Parameters.Add(new NpgsqlParameter("level", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (int)level });

        // Adiciona o parâmetro 'value' com o JSON
        command.Parameters.AddWithValue("value", NpgsqlTypes.NpgsqlDbType.Jsonb, json);

        // Executa o comando
        command.ExecuteNonQuery();

        // Retorna o ID gerado
        return id;
    }

    internal List<dynamic> Search(string table, SearchObject query)
    {
    // Cria o comando de consulta com um intervalo de datas
    using var command = new NpgsqlCommand(@$"
        SELECT id, level, created_at, content 
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
    DateTime datetime1 = DateTime.SpecifyKind(query.datetime1, DateTimeKind.Utc);
    DateTime datetime2 = DateTime.SpecifyKind(query.datetime2, DateTimeKind.Utc);
    command.Parameters.Add(new NpgsqlParameter("datetime1", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = datetime1 });
    command.Parameters.Add(new NpgsqlParameter("datetime2", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = datetime2 });

    command.Parameters.Add(new NpgsqlParameter("take", NpgsqlTypes.NpgsqlDbType.Integer) { Value = query.take });
    command.Parameters.Add(new NpgsqlParameter("skip", NpgsqlTypes.NpgsqlDbType.Integer) { Value = query.skip });

    command.Parameters.Add(new NpgsqlParameter("search", NpgsqlTypes.NpgsqlDbType.Text) { Value = $"%{query.search}%" });


    // Executa o comando e obtém o resultado
    using var reader = command.ExecuteReader();

    // Cria uma lista para armazenar os resultados
    var results = new List<dynamic>();

    // Percorre todos os registros retornados
    while (reader.Read())
    {
        // Cria um objeto dinâmico para armazenar os valores da linha
        var record = new
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            Level = ((Level)reader.GetInt64(reader.GetOrdinal("level"))).ToString(),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            Content = System.Text.Json.JsonSerializer.Deserialize<dynamic>(reader["content"].ToString())
        };

        // Adiciona o objeto dinâmico à lista de resultados
        results.Add(record);
    }

    // Retorna a lista de resultados
    return results;
}


    public void DeleteRecords(string table, DateTime cutdate)
    {
        // Prepara o comando de deletar
        using var command = new NpgsqlCommand(@$"
            DELETE FROM {table} 
            WHERE created_at < @cutdate;", _conn);

        // Define o parâmetro 'cutdate' como 'TIMESTAMP WITH TIME ZONE'
        command.Parameters.Add(new NpgsqlParameter("cutdate", NpgsqlTypes.NpgsqlDbType.TimestampTz)
        {
            Value = DateTime.SpecifyKind(cutdate, DateTimeKind.Utc) // Certifique-se que a data é UTC
        });

        // Executa o comando de exclusão
        command.ExecuteNonQuery();
    }

    internal object GetByID(string table, long id)
    {
        using var command = new NpgsqlCommand(@$"
            SELECT id, level, created_at, content 
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
            var record = new
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                Level = ((Level)reader.GetInt64(reader.GetOrdinal("level"))).ToString(),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                Content = System.Text.Json.JsonSerializer.Deserialize<dynamic>(reader["content"].ToString())
            };

            // Adiciona o objeto dinâmico à lista de resultados
            return record;
        }

        // Retorna a lista de resultados
        return null;
    }
}

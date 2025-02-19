using System.Text.RegularExpressions;
using Npgsql;
using server.Domain;
using SnowflakeID;

using Dapper;

namespace server.Repositories;

public class DBRepository : IDisposable
{
    public DBRepository(NpgsqlConnection connection)
    {
        _conn = connection;
        _conn.Open();
    }

    NpgsqlConnection _conn;
    private TimeZoneInfo _tz = TimeZoneInfo.Utc;

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
            SELECT substr(table_name, 5) as table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
            AND table_type = 'BASE TABLE'
            and substr(table_name, 0, 5) = 'log_';", _conn);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            // Adiciona o nome da tabela à lista
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    public bool TableExists(string table)
    {
        using var command = new NpgsqlCommand(@"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_type = 'BASE TABLE' 
            AND table_name = @table", _conn);

        command.Parameters.AddWithValue("table", $"log_{table}");

        var result = (long)command.ExecuteScalar();
        if(result == 0){
            throw new Exception($"Doesn't exist a table named '{table}'");
        }

        return result > 0;
    }

    public void CreateTable(string table)
    {
        ValidateTable(table);

        string txt_command = @$"CREATE TABLE log_{table} (
                                id UUID PRIMARY KEY,
                                level SMALLINT CHECK (level >= 0 AND level <= 9),
                                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                description VARCHAR(255) NULL,
                                content jsonb
                            );";
        using var command = new NpgsqlCommand(txt_command, _conn);
        command.ExecuteNonQuery();
    }

    
    public void DropTable(string table)
    {
        string txt_command = $"DROP TABLE IF EXISTS log_{table};";
        using var command = new NpgsqlCommand(txt_command, _conn);
        command.ExecuteNonQuery();
    }




    public void CreateJsonbIndex(string table){
        string txt_command = 
            @$"CREATE INDEX idx_{table} ON log_{table} USING GIN (content jsonb_ops);";
        using var command = new NpgsqlCommand(txt_command, _conn);
        command.ExecuteNonQuery();
    }

    public void CreateDescriptionbIndex(string table){
        string txt_command = 
            @$"CREATE INDEX idx_{table}_desc ON log_{table} USING GIN (description gin_trgm_ops);";
        using var command = new NpgsqlCommand(txt_command, _conn);
        command.ExecuteNonQuery();
    }

    public void CreateDateTimeIndex(string table){
        string txt_command = 
            @$"CREATE INDEX idx_{table}_created_at ON log_{table} (created_at);";
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

    public Guid Insert(string table, Level level, string description, string json)
    {
        ValidateTable(table);

        // Gera o ID Snowflake
        Guid id = SnowflakeGuid.NewGuid();

        // Cria o comando de inserção
        using var command = new NpgsqlCommand($"INSERT INTO log_{table} (id, level, description, content) VALUES (@id, @level, @description, @value::jsonb)", _conn);

        // Define o parâmetro 'id' explicitamente como BIGINT
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = id });

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
        FROM log_{table} 
        WHERE 1=1 
        and created_at BETWEEN @datetime1 AND @datetime2
        and (content::text ILIKE @search OR description::text ILIKE @search)
        --
        ORDER BY id
        LIMIT @take -- Toma 20 registros
        OFFSET @skip
        ", _conn);

    // Define os parâmetros para datetime1 e datetime2
        DateTime datetime1 = DateTime.UtcNow.AddHours(-1);
        if(query.datetime1 > DateTime.MinValue){
            datetime1 = query.datetime1.Add(-_tz.GetUtcOffset(query.datetime1));
        }
        command.Parameters.Add(new NpgsqlParameter("datetime1", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = datetime1.ToUniversalTime() });

        DateTime datetime2 = DateTime.UtcNow;
        if(query.datetime2 > DateTime.MinValue){
            datetime2 = query.datetime2.Add(-_tz.GetUtcOffset(query.datetime2));
        }
        command.Parameters.Add(new NpgsqlParameter("datetime2", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = datetime2.ToUniversalTime() });


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
            id = reader.GetGuid(reader.GetOrdinal("id")),
            level = (Level)reader.GetInt64(reader.GetOrdinal("level")),
            description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
            created_at = reader.GetDateTime(reader.GetOrdinal("created_at")),
            content = System.Text.Json.JsonSerializer.Deserialize<dynamic>(reader["content"].ToString())
        };

        record.created_at = record.created_at.Add(_tz.GetUtcOffset(record.created_at)); //correcting the timezone

        // Adiciona o objeto dinâmico à lista de resultados
        results.Add(record);
    }

    // Retorna a lista de resultados
    return results.OrderByDescending(x => x.id).ToList();
}


    public void DeleteRecords(string table, DateTime before_date)
    {
        Console.WriteLine($"It will remove records added before {before_date.ToString("yyyy/MM/dd")} from table {table}. If you don't like that, you can edit the configuration. See the session 'Table Recycling' on https://github.com/hudsonventura/LogCenter");
        // Prepara o comando de deletar
        using var command = new NpgsqlCommand(@$"
            DELETE FROM log_{table} 
            WHERE created_at < @before_date;", _conn);

        // Define o parâmetro 'cutdate' como 'TIMESTAMP WITH TIME ZONE'
        command.Parameters.Add(new NpgsqlParameter("before_date", NpgsqlTypes.NpgsqlDbType.TimestampTz)
        {
            Value = DateTime.SpecifyKind(before_date, DateTimeKind.Utc) // Certifique-se que a data é UTC
        });


        int effected = command.ExecuteNonQuery();
        Console.WriteLine($"Removed {effected} records from table log_{table}");
    }

    internal Record GetByID(string table, Guid id)
    {
        string query = @$"SELECT * FROM log_{table} WHERE id = @id";
        Record result = _conn.QueryFirstOrDefault<Record>(query, new { id });

        if (result != null && !string.IsNullOrEmpty(result.content))
        {
            result.content = System.Text.Json.JsonSerializer.Deserialize<dynamic>(result.content);
        }

        return result;

    }


    public void SetTimezone(string timezone)
    {
        _tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
    }

        /// <summary>
        /// Insert or update a table config
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="configs">ConfigTableObject</param>
    internal void UpsertConfig(string table, ConfigTableObject configs)
    {
        using var command = new NpgsqlCommand(@$"
            INSERT INTO config (table_name, delete, delete_input, vacuum, vacuum_input, vacuum_full, vacuum_Full_input) 
            VALUES (@table, @delete, @delete_input, @vacuum, @vacuum_input, @vacuum_full, @vacuum_Full_input) 
            ON CONFLICT (table_name) 
            DO UPDATE SET 
                delete = @delete,
                delete_input = @delete_input,
                vacuum = @vacuum,
                vacuum_input = @vacuum_input,
                vacuum_full = @vacuum_full,
                vacuum_Full_input = @vacuum_Full_input", _conn);

        command.Parameters.Add(new NpgsqlParameter("table", NpgsqlTypes.NpgsqlDbType.Text) { Value = table });
        command.Parameters.Add(new NpgsqlParameter("delete", NpgsqlTypes.NpgsqlDbType.Boolean) { Value = configs.delete });
        command.Parameters.Add(new NpgsqlParameter("delete_input", NpgsqlTypes.NpgsqlDbType.Integer) { Value = configs.delete_input });
        command.Parameters.Add(new NpgsqlParameter("vacuum", NpgsqlTypes.NpgsqlDbType.Boolean) { Value = configs.vacuum });
        command.Parameters.Add(new NpgsqlParameter("vacuum_input", NpgsqlTypes.NpgsqlDbType.Text) { Value = configs.vacuum_input });
        command.Parameters.Add(new NpgsqlParameter("vacuum_full", NpgsqlTypes.NpgsqlDbType.Boolean) { Value = configs.vacuum_full });
        command.Parameters.Add(new NpgsqlParameter("vacuum_Full_input", NpgsqlTypes.NpgsqlDbType.Text) { Value = configs.vacuum_full_input });

        command.ExecuteNonQuery();
    }


    internal void CreateConfigTable()
    {
        using var command = new NpgsqlCommand(@$"
            CREATE TABLE IF NOT EXISTS config (
                table_name text PRIMARY KEY,
                delete boolean NOT NULL DEFAULT false,
                delete_input integer,
                vacuum boolean NOT NULL DEFAULT false,
                vacuum_input text,
                vacuum_full boolean NOT NULL DEFAULT false,
                vacuum_Full_input text
            );", _conn);

        command.ExecuteNonQuery();
    }



    internal List<ConfigTableObject> GetConfiguredTables()
    {
        using var command = new NpgsqlCommand("SELECT * FROM config", _conn);
        using var reader = command.ExecuteReader();

        var tables = new List<ConfigTableObject>();
        while (reader.Read())
        {
            tables.Add(new ConfigTableObject
            {
                table_name = reader.GetString(0),
                delete = reader.GetBoolean(1),
                delete_input = reader.GetInt32(2),
                vacuum = reader.GetBoolean(3),
                vacuum_input = reader.IsDBNull(4) ? null : reader.GetString(4),
                vacuum_full = reader.GetBoolean(5),
                vacuum_full_input = reader.IsDBNull(6) ? null : reader.GetString(6),
            });
        }

        return tables;
    }



    internal void VacuumTable(string table_name)
    {
        using var command = new NpgsqlCommand($"VACUUM log_{table_name}", _conn);
        command.ExecuteNonQuery();
    }

    internal void VacuumFullTable(string table_name)
    {
        using var command = new NpgsqlCommand($"VACUUM FULL log_{table_name}", _conn);
        command.ExecuteNonQuery();
    }

}


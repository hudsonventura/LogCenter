using System.Text.RegularExpressions;
using Npgsql;
using server.Domain;


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
        string query = @"
        SELECT substr(table_name, 5) as table_name
        FROM information_schema.tables
        WHERE table_schema = 'public'
        AND table_type = 'BASE TABLE'
        AND substr(table_name, 0, 5) = 'log_';";

        return _conn.Query<string>(query).ToList();
    }

    public bool TableExists(string table)
    {
        string query = @"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_type = 'BASE TABLE' 
            AND table_name = @table";

        long result = _conn.ExecuteScalar<long>(query, new { table = $"log_{table}" });

        if (result == 0)
        {
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
                                message VARCHAR(255) NOT NULL,
                                correlation VARCHAR(100) NULL,
                                content JSONB NULL
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

    public void CreateCorrelationIndex(string table){
        string txt_command = 
            @$"CREATE INDEX idx_{table}_correlation ON log_{table} USING GIN (correlation gin_trgm_ops);";
        using var command = new NpgsqlCommand(txt_command, _conn);
        command.ExecuteNonQuery();
    }
    
    public void CreateMessageIndex(string table){
        string txt_command = 
            @$"CREATE INDEX idx_{table}_message ON log_{table} USING GIN (message gin_trgm_ops);";
        using var command = new NpgsqlCommand(txt_command, _conn);
        command.ExecuteNonQuery();
    }

    public void CreateDateTimeIndex(string table){
        string txt_command = 
            @$"CREATE INDEX idx_{table}_created_at ON log_{table} (created_at);";
        using var command = new NpgsqlCommand(txt_command, _conn);
        command.ExecuteNonQuery();
    }





    public void ValidateTable(string table)
    {
        var regex = new Regex(@"[!@#$%^&*(),.?""{}|<>]");
        if(regex.IsMatch(table)){
            throw new Exception("The index must be a string without special chars");
        }

    }

    public Guid Insert(string table, Level level, string correlation, string message, string json)
    {
        ValidateTable(table);

        // Gera o ID Snowflake
        Guid id = SnowflakeGuid.NewGuid();

        // Cria o comando de inserção
        using var command = new NpgsqlCommand($"INSERT INTO log_{table} (id, level, correlation, message, content) VALUES (@id, @level, @correlation, @message, @value::jsonb)", _conn);

        // Define o parâmetro 'id' explicitamente como BIGINT
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = id });
        command.Parameters.Add(new NpgsqlParameter("level", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (int)level });
        command.Parameters.Add(new NpgsqlParameter("correlation", NpgsqlTypes.NpgsqlDbType.Text) { Value = correlation ?? (object)DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("message", NpgsqlTypes.NpgsqlDbType.Text) { Value = message });

        // Adiciona o parâmetro 'value' com o JSON
        command.Parameters.Add(new NpgsqlParameter("value", NpgsqlTypes.NpgsqlDbType.Jsonb) { Value = json != null ? json : DBNull.Value });

        // Executa o comando
        command.ExecuteNonQuery();

        // Retorna o ID gerado
        return id;
    }

    internal List<Record> Search(string table, SearchObject query)
    {
        
        string sql = @$"
            SELECT id, level, created_at AT TIME ZONE 'UTC' as created_at, correlation, message, content 
            FROM log_{table} 
            WHERE created_at AT TIME ZONE 'UTC' BETWEEN @datetime1 AND @datetime2
            AND (content::text ILIKE @search OR correlation::text ILIKE @search OR message::text ILIKE @search)
            ORDER BY id DESC
            LIMIT @take 
            OFFSET @skip";

        DateTime datetime1 = (query.datetime1 == DateTime.MinValue) ? DateTime.UtcNow.AddHours(-1) : datetime1 = query.datetime1.Add(-_tz.GetUtcOffset(query.datetime1));
        DateTime datetime2 = (query.datetime2 == DateTime.MinValue) ? DateTime.UtcNow : query.datetime2.Add(-_tz.GetUtcOffset(query.datetime2));

        var parameters = new
        {
            datetime1 = DateTime.SpecifyKind(datetime1, DateTimeKind.Utc),
            datetime2 = DateTime.SpecifyKind(datetime2, DateTimeKind.Utc),
            search = $"%{query.search}%",
            take = query.take,
            skip = query.skip
        };

        var results = _conn.Query<Record>(sql, parameters).ToList();

        // Ajustando timezone manualmente após a consulta
        foreach (var record in results)
        {
            record.created_at = record.created_at.Add(_tz.GetUtcOffset(record.created_at));
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
        string query = "SELECT * FROM config";
        return _conn.Query<ConfigTableObject>(query).ToList();
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

    internal List<ConfigTableObject> GetConfigTables()
    {
        string query = @"SELECT 
                            substr(t.table_name, 5) AS table_name,
                            pg_total_relation_size(quote_ident(t.table_name)) AS size, c.*
                        FROM information_schema.tables t
                        left JOIN config c ON c.table_name = substr(t.table_name, 5)
                        WHERE table_schema = 'public'
                        AND table_type = 'BASE TABLE'
                        AND substr(t.table_name, 0, 5) = 'log_';";

        return _conn.Query<ConfigTableObject>(query).ToList();
    }

    internal Guid GetMaxID(string table)
    {
        string query = $"SELECT id FROM log_{table} ORDER BY id DESC LIMIT 1;";

        return _conn.Query<Guid>(query).FirstOrDefault();
    }

    internal ConfigTableObject GetConfigTable(string table)
    {
        var result = GetConfigTables();
        return result.FirstOrDefault(x => x.table_name == table);
    }
}


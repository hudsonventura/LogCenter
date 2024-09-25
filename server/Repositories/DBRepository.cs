using System.Text.RegularExpressions;
using Npgsql;
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

    public void CreateIndexTable(string index){
        string txt_command = @$"CREATE TABLE {index} (
                                id BIGINT PRIMARY KEY,
                                created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                content jsonb
                            );";
        using var command = new NpgsqlCommand(txt_command, _conn);
        command.ExecuteNonQuery();
    }

    public void DeleteIndexTable(string index)
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




    public void ValidateIndex(string index)
    {
        var regex = new Regex(@"[!@#$%^&*(),.?""{}|<>]");
        if(regex.IsMatch(index)){
            throw new Exception("The index must be a string without special chars");
        }

    }

    public ulong Insert(string index, string json)
    {
        // Gera o ID Snowflake
        ulong id = SnowflakeIDGenerator.GetSnowflake(0).ToUInt64();

        // Cria o comando de inserção
        using var command = new NpgsqlCommand($"INSERT INTO {index} (id, content) VALUES (@id, @value::jsonb)", _conn);

        // Define o parâmetro 'id' explicitamente como BIGINT
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Bigint) { Value = (long)id });

        // Adiciona o parâmetro 'value' com o JSON
        command.Parameters.AddWithValue("value", NpgsqlTypes.NpgsqlDbType.Jsonb, json);

        // Executa o comando
        command.ExecuteNonQuery();

        // Retorna o ID gerado
        return id;
    }
}

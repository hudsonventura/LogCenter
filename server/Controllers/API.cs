using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace server.Controllers;

/// <summary>
/// DOC 1
/// </summary>
[ApiController]
public class API : ControllerBase
{

    private readonly NpgsqlConnection _connection;

    public API(NpgsqlConnection connection)
    {
        _connection = connection;
        _connection.Open();
    }


    /// <summary>
    /// Save a message to index log
    /// </summary>
    /// <returns></returns>
    [HttpPost("/{index}/_doc")]
    public ActionResult Index_Doc(string index, [FromBody] dynamic obj)
    {
        return Index(index, obj);
    }
    /// <summary>
    /// Save a message to index log
    /// </summary>
    /// <returns></returns>
    [HttpPost("/{index}")]
    public ActionResult Index(string index, [FromBody] dynamic obj)
    {
        ValidateIndex(index);
        var body = Utils.Base64Replacer.ReplaceBase64Content(obj);
        var json = JsonSerializer.Serialize(body);


        try
        {
            //tenta salvar na tabela do meu index.
            //se der certo, 200
            TryToSave(index, json);
            return Ok();
        }
        catch (System.Exception error1)
        {
            //se der erro, verificar que tipo de erro.
            //se a tabela nào existir, entao cria.
            try
            {
                Console.Write($"Creating new table ({index}) ... ");
                CreateIndexTable(index);
                Console.Write("OK! ... ");

                Console.Write($"Creating new index ({index}) ... ");
                CreateIndex(index);
                Console.Write("OK! ... ");

                TryToSave(index, json);
                return Ok();
            }
            catch (System.Exception error2)
            {
                Console.WriteLine(error2.Message);
                return StatusCode(500, error2.Message);
            }
        }
        return StatusCode(500);
    }

    private void ValidateIndex(string index)
    {
        var regex = new Regex(@"[!@#$%^&*(),.?""{}|<>]");
        if(regex.IsMatch(index)){
            throw new Exception("The index must be a string without special chars");
        }

    }

    private int TryToSave(string index, string json)
    {

        using var command = new NpgsqlCommand($"INSERT INTO {index} (content) VALUES (@value::jsonb)", _connection);
        command.Parameters.AddWithValue("value", json);

        return command.ExecuteNonQuery();
    }

    private void CreateIndexTable(string index){
        string txt_command = @$"CREATE TABLE {index} (
                                id SERIAL PRIMARY KEY,
                                content jsonb
                            );";
        using var command = new NpgsqlCommand(txt_command, _connection);
        command.ExecuteNonQuery();
    }

    private void CreateIndex(string index){
        string txt_command = 
            @$"CREATE INDEX idx_{index} ON {index} USING GIN (content jsonb_ops);
";
        using var command = new NpgsqlCommand(txt_command, _connection);
        command.ExecuteNonQuery();
    }

}

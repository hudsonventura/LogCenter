using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using server.Repositories;
using System.Text.Json.Nodes;
using server.Domain;

namespace server.Controllers;

/// <summary>
/// DOC 1
/// </summary>
[ApiController]
public class API : ControllerBase
{

    private readonly DBRepository _db;

    public API(DBRepository db)
    {
        _db = db;
    }


    /// <summary>
    /// Save a message to table log
    /// </summary>
    /// <returns></returns>
    [HttpPost("/{table}/_doc")]
    public ActionResult Index_Doc(string table, [FromBody] dynamic obj)
    {
        return Index(table, obj);
    }

    /// <summary>
    /// Save a message to table log
    /// </summary>
    /// <returns></returns>
    [HttpPost("/{table}")]
    public ActionResult Index(string table, [FromBody] dynamic obj)
    {
        _db.ValidateIndex(table);
        table = table.Replace(" ", "_");
        
        var json = string.Empty;
        try
        {
            dynamic body = Utils.Base64Replacer.ReplaceBase64Content(obj);
            json = JsonSerializer.Serialize(body);
        }
        catch (System.Exception)
        {
            json = JsonSerializer.Serialize(obj);
        }
        


        try
        {
            //tenta salvar na tabela do meu index.
            //se der certo, 200
            var id = _db.Insert(table, json);
            return Created("teste", id);
        }
        catch (System.Exception error1)
        {
            //se der erro, verificar que tipo de erro.
            //se a tabela nào existir, entao cria.
            try
            {
                Console.Write($"Creating new table ({table}) ... ");
                _db.CreateIndexTable(table);
                Console.Write("OK! ... ");

                Console.Write($"Creating new JSONB index ({table}) ... ");
                _db.CreateJsonbIndex(table);
                Console.Write("OK! ... ");

                var id = _db.Insert(table, json);
                return Created("teste", id);
            }
            catch (System.Exception error2)
            {
                Console.WriteLine(error2.Message);
                return StatusCode(500, error2.Message);
            }
        }
        return StatusCode(500);
    }

    

    /// <summary>
    /// Delete table log
    /// </summary>
    /// <returns></returns>
    [HttpDelete("/{table}/_doc")]
    public ActionResult Delete_Doc(string table)
    {
        return Delete(table);
    }

    /// <summary>
    /// Delete table log
    /// </summary>
    /// <returns></returns>
    [HttpDelete("/{table}")]
    public ActionResult Delete(string table)
    {
        _db.ValidateIndex(table);
        table = table.Replace(" ", "_");
        _db.DeleteIndexTable(table);

        return NoContent();
    }



    [HttpGet("/Search/{table}")]
    public ActionResult Search(string table, [FromQuery] SearchObject query){
        var response = _db.Search(table, query);
        if(response.Count() == 0){
            return NoContent();
        }
        return Ok(response);
        
    }

}

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
public class RecordController : ControllerBase
{

    private readonly DBRepository _db;

    public RecordController(DBRepository db)
    {
        _db = db;
    }


    


    /// <summary>
    /// Save a message string or object json to table log. Returns a snowflake id
    /// </summary>
    /// <returns>long</returns>
    [HttpPost("/{table}/_doc")]
    public ActionResult<long> Insert_Doc(string table, [FromBody] dynamic obj, [FromHeader] Level level = Level.Info)
    {
        return Insert(table, obj, level);
    }

    /// <summary>
    /// Save a message string or object json to table log. Returns a snowflake id
    /// </summary>
    /// <returns>long</returns>
    [HttpPost("/{table}")]
    public ActionResult<long> Insert(string table, [FromBody] dynamic obj, [FromHeader] Level level = Level.Info)
    {
        _db.ValidateTable(table);
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
            var id = _db.Insert(table, level, json);
            return Created("teste", id);
        }
        catch (System.Exception error1)
        {
            //se der erro, verificar que tipo de erro.
            //se a tabela nào existir, entao cria.
            try
            {
                Console.Write($"Creating new table ({table}) ... ");
                _db.CreateTable(table);
                Console.Write("OK! ... ");

                Console.Write($"Creating new JSONB index ({table}) ... ");
                _db.CreateJsonbIndex(table);
                Console.Write("OK! ... ");

                var id = _db.Insert(table, level, json);
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
    /// Delete records from table log before a cutoff date
    /// </summary>
    /// <returns></returns>
    [HttpDelete("/{table}")]
    public ActionResult Delete(string table, [FromQuery] DateTime datecut)
    {
        _db.ValidateTable(table);
        table = table.Replace(" ", "_");
        _db.DeleteRecords(table, datecut);

        return NoContent();
    }



    /// <summary>
    /// Search string or json object into a table. Returns a list of record
    /// </summary>
    /// <param name="table"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpGet("/Search/{table}")]
    public ActionResult<List<Record>> Search(string table, [FromQuery] SearchObject query){
        var response = _db.Search(table, query);
        if(response.Count() == 0){
            return NoContent();
        }
        return Ok(response);
    }


    /// <summary>
    /// Get a record by ID. Returns a record
    /// </summary>
    /// <param name="table"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("/{table}/{id}")]
    public ActionResult GetByID(string table, long id){
        var response = _db.GetByID(table, id);
        if(response == null){
            return NotFound();
        }
        return Ok(response);
    }
}

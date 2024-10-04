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
    /// <param name="table">Table name</param>
    /// <param name="obj">Message string or object json</param>
    /// <param name="level">Log level. Default is Info</param>
    /// <returns>long</returns>
    [HttpPost("/{table}/_doc")]
    public ActionResult<long> Insert_Doc(string table, [FromBody] dynamic obj, [FromHeader] Level level = Level.Info, [FromHeader] string? description = null)
    {
        return Insert(table, obj, level, description);
    }

    /// <summary>
    /// Save a message string or object json to table log. Returns a snowflake id
    /// </summary>
    /// <param name="table">Table name</param>
    /// <param name="obj">Message string or object json</param>
    /// <param name="level">Log level. Default is Info</param>
    /// <returns>long</returns>
    [HttpPost("/{table}")]
    public ActionResult<long> Insert(string table, [FromBody] dynamic obj, [FromHeader] Level level = Level.Info, [FromHeader] string? description = null)
    {
        _db.ValidateTable(table);
        table = table.Replace(" ", "_").ToLower();
        
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
            var id = _db.Insert(table, level, description, json);
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

                var id = _db.Insert(table, level, description, json);
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
    /// Delete records from table log before a cutoff date. It will mark the records to be deleted, but will not delete them at the moment. They will be deleted in a few minutes
    /// </summary>
    /// <param name="table">Table name</param>
    /// <param name="before_date">Remove records inserted before a date</param>
    /// <returns></returns>
    [HttpDelete("/{table}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult Delete(string table, [FromQuery] DateTime before_date)
    {
        _db.ValidateTable(table);
        table = table.Replace(" ", "_").ToLower();
        
        try
        {
            _db.DeleteRecords(table, before_date);
            return Ok();
        }
        catch (System.Exception error)
        {
            return StatusCode(500, error.Message);
        }
    }



    /// <summary>
    /// Search string or json object into a table. Returns a list of record
    /// </summary>
    /// <param name="table">Table name</param>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpGet("/{table}")]
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
    /// <param name="table">Table name</param>
    /// <param name="id">Record ID (Snowflake)</param>
    /// <returns></returns>
    /// <summary>
    /// Get a record by ID. Returns a record
    /// </summary>
    /// <param name="table"></param>
    /// <param name="id"></param>
    /// <returns>200 - Record, 204 - No Content, 500 - Internal Server Error</returns>
    [HttpGet("/{table}/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Record))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Record> GetByID(string table, long id){
        var response = _db.GetByID(table, id);
        if(response == null){
            return NoContent();
        }
        return Ok(response);
    }
}

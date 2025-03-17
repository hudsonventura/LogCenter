using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using server.Repositories;
using System.Text.Json.Nodes;
using server.Domain;
using Microsoft.AspNetCore.Authorization;

namespace server.Controllers;

/// <summary>
/// DOC 1
/// </summary>
[Authorize]
[ApiController]
public class RecordController : ControllerBase
{

    private readonly DBRepository _db;
    private readonly LastRecordIDRepository _lastID;
    private readonly TokenRepository _token;

    public RecordController(DBRepository db, LastRecordIDRepository lastID, TokenRepository token)
    {
        _db = db;
        _lastID = lastID;
        _token = token;
    }


    




    /// <summary>
    /// Save a message string or object json to table log. Returns a snowflake id
    /// </summary>
    /// <param name="table">Table name</param>
    /// <param name="obj">Message string or object json</param>
    /// <param name="level">Log level. Default is Info</param>
    /// <returns>uuid</returns>
    [HttpPost("/{table}")]
    public ActionResult<Guid> Insert(
        string table, 
        [FromHeader] string message, 
        [FromHeader] DateTime? timestamp = null,
        [FromHeader] Level level = Level.Info, 
        [FromHeader] string? TraceId = null,
        [FromBody] dynamic obj = null)
    {
        if(timestamp == null){
            timestamp = DateTime.UtcNow;
        }
        table = table.Replace(" ", "_").ToLower();
        _db.ValidateTable(table);

        if(!_token.CheckTableAccess(User, table)){
            return Unauthorized();
        }
        
        string json = null;
        if(!(obj is JsonElement { ValueKind: JsonValueKind.Null }) && obj is not null) {
            try
            {
                dynamic body = Utils.Base64Replacer.ReplaceBase64Content(obj);
                json = JsonSerializer.Serialize(body);
            }
            catch (System.Exception)
            {
                try
                {
                    json = JsonSerializer.Serialize(obj);
                }
                catch (System.Exception)
                {
                    
                }
            }
        }

        TraceId = (TraceId.Length >= 100) ? TraceId.Substring(0, 100) : TraceId;
        
        
        Guid id = Guid.Empty;
        try
        {
            //tenta salvar na tabela do meu index.
            //se der certo, 200
            id = _db.Insert(table, level, TraceId, message, json, timestamp);
            return Created(id.ToString(), id);
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

                Console.Write($"Creating new TRACEID index ({table}) ... ");
                _db.CreateTraceIdIndex(table);
                Console.Write("OK! ... ");

                Console.Write($"Creating new MESSAGE index ({table}) ... ");
                _db.CreateMessageIndex(table);
                Console.Write("OK! ... ");
                
                Console.Write($"Creating new JSONB index ({table}) ... ");
                _db.CreateJsonbIndex(table);
                Console.Write("OK! ... ");

                Console.Write($"Creating new DATETIME index ({table}) ... ");
                _db.CreateDateTimeIndex(table);
                Console.Write("OK! ... ");

                id = _db.Insert(table, level, TraceId, message, json, timestamp);
                return Created($"/{table}/{id}", $"/{table}/{id}");
            }
            catch (System.Exception error2)
            {
                Console.WriteLine(error2.Message);
                return StatusCode(500, error2.Message);
            }
        }
        finally            
        {
            if (id != Guid.Empty)
            {
                _lastID.SetMaxID(table, id);
            }

        }
        return StatusCode(500);
    }

    


    

    /// <summary>
    /// Remove older records manually from a table (see table config).
    /// </summary>
    /// <param name="table">Table name</param>
    /// <param name="before_date">Remove records inserted before a date</param>
    /// <returns></returns>
    [HttpDelete("/{table}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult Delete(string table, [FromQuery] DateTime before_date)
    {
        try
        {
            table = table.Replace(" ", "_").ToLower();
            _db.TableExists(table);
            if(!_token.CheckTableAccess(User, table)){
                return Unauthorized();
            }
        }
        catch (System.Exception error)
        {
            return BadRequest(error.Message);
        }
        
        
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
    /// <param name="timezone">Integer value of your timezone. Default is 0. Ex.: -3 (represents timezone of Sao_Paulo)</param>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpGet("/{table}")]
    public ActionResult<List<Record>> Search(string table, [FromQuery] SearchObject query, [FromHeader] string timezone){
        _db.SetTimezone(timezone); 
        var response = _db.Search(table, query);
        if(response.Count() == 0){
            return NoContent();
        }
        return Ok(response.OrderByDescending(x => x.timestamp).ToList());
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
    /// <param name="timezone">Integer value of your timezone. Default is 0. Ex.: -3 (represents timezone of Sao_Paulo)</param>
    /// <returns>200 - Record, 204 - No Content, 500 - Internal Server Error</returns>
    [HttpGet("/{table}/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Record))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Record> GetByID(string table, Guid id, [FromHeader] string timezone = "UTC"){
        _db.SetTimezone(timezone); 
        var response = _db.GetByID(table, id);
        if(response == null){
            return NoContent();
        }
        return Ok(response);
    }



    [HttpGet("/{table}/Last")]
    public ActionResult<Guid> Last(string table)
    {
        try
        {
            return Ok(_lastID.GetMaxID(table));
        }
        catch (System.Exception error)
        {
            return BadRequest(error.Message);
        }
        
    }
}

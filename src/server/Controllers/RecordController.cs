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
    /// Save a message string and object json to table log. Returns a Guid
    /// * is optional
    /// </summary>
    /// <param name="table">Table name</param>
    /// <param name="obj">*Object json to be saved</param>
    /// <returns>uuid</returns>
    [HttpPost("/{table}")]
    public ActionResult<Guid> Insert(
        string table, 
        [FromBody] RequestRecord obj = null)
    {
        obj ??= new RequestRecord();

        if(obj.Timestamp == default){
            obj.Timestamp = DateTime.UtcNow;
        }
        table = table.Replace(" ", "_").ToLower();
        _db.ValidateTable(table);

        if(!_token.CheckTableAccess(User, table)){
            return Unauthorized();
        }
        
        string json = null;
        if(obj.Content is JsonElement { ValueKind: not JsonValueKind.Null and not JsonValueKind.Undefined } contentElement) {
            try
            {
                dynamic body = Utils.Base64Replacer.ReplaceBase64Content(contentElement);
                json = JsonSerializer.Serialize(body);
            }
            catch (System.Exception)
            {
                try
                {
                    json = JsonSerializer.Serialize(obj.Content);
                }
                catch (System.Exception)
                {
                    
                }
            }
        }
        else if(obj.Content is not null)
        {
            try
            {
                json = JsonSerializer.Serialize(obj.Content);
            }
            catch (System.Exception)
            {
            }
        }

        if(obj.TraceId != null){
            obj.TraceId = (obj.TraceId?.Length >= 100) ? obj.TraceId.Substring(0, 100) : obj.TraceId;
        }
        
        
        
        Guid id = Guid.Empty;
        try
        {
            //tenta salvar na tabela do meu index.
            //se der certo, 200
            id = _db.Insert(table, obj.Level, obj.TraceId, obj.Message, json, obj.Timestamp);
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

                id = _db.Insert(table, obj.Level, obj.TraceId, obj.Message, json, obj.Timestamp);
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
    public ActionResult<object> Delete(string table, [FromQuery] DateTime before_date)
    {
        string executionId = Guid.NewGuid().ToString("N");
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
            _db.InsertMaintenanceLog(
                RecordLevel.Information,
                $"Manual delete started for table '{table}'",
                new
                {
                    action = "delete_started",
                    origin = "manual",
                    table,
                    before_date
                },
                executionId);

            int rowsAffected = _db.DeleteRecords(table, before_date);

            _db.InsertMaintenanceLog(
                RecordLevel.Information,
                $"Manual delete finished for table '{table}' affecting {rowsAffected} rows",
                new
                {
                    action = "delete_finished",
                    origin = "manual",
                    table,
                    before_date,
                    rows_affected = rowsAffected
                },
                executionId);
            return Ok(new
            {
                rows_affected = rowsAffected
            });
        }
        catch (System.Exception error)
        {
            _db.InsertMaintenanceLog(
                RecordLevel.Error,
                $"Manual delete failed for table '{table}'",
                new
                {
                    action = "delete_failed",
                    origin = "manual",
                    table,
                    before_date,
                    error = error.Message
                },
                executionId);
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
    public ActionResult<List<Record>> Search(string table, [FromQuery] SearchObject query, [FromHeader] string timezone = "UTC"){
        _db.SetTimezone(timezone); 
        var response = _db.Search(table, query);
        if(response.Count() == 0){
            return NoContent();
        }
        return Ok(response.OrderByDescending(x => x.Timestamp).ToList());
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

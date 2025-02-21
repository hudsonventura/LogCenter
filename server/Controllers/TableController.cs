using Microsoft.AspNetCore.Mvc;
using server.Domain;
using server.Repositories;

namespace server.Controllers;

/// <summary>
/// Operations under a table
/// </summary>
[Route("[controller]")]
public class TableController : Controller
{
    private readonly DBRepository _db;

    public TableController(DBRepository db)
    {
        _db = db;
    }

    /// <summary>
    /// List tables for frontend use
    /// </summary>
    /// <returns>200 - List of tables, 500 - Internal Server Error</returns>
    [HttpGet("/ListTables")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<List<string>> ListTabels()
    {
        try
        {
            var tables = _db.ListTabels();
            if (tables.Count() == 0)
            {
                return NoContent();
            }
            return Ok(tables);
        }
        catch (System.Exception)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }





    /// <summary>
    /// Drop the table log
    /// </summary>
    /// <returns></returns>
    [HttpDelete("/Drop/{table}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult DropTable(string table)
    {
        table = table.Replace(" ", "_").ToLower();
        _db.ValidateTable(table);
        try
        {
            _db.DropTable(table);
            return Ok();
        }
        catch (System.Exception)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    /// <summary>
    /// Insert or update a table config
    /// </summary>
    /// <param name="configs"></param>
    /// <param name="table"></param>
    /// <returns>ConfigTableObject</returns>
    [HttpPut("/Config/{table}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ConfigTableObject))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<ConfigTableObject> ConfigTable([FromBody]ConfigTableObject configs, string table)
    {
        configs.Validate();
        table = table.Replace(" ", "_").ToLower();
        _db.TableExists(table);
        _db.UpsertConfig(table, configs);
        return Ok(configs);
    }



    /// <summary>
    /// Get table config
    /// </summary>
    /// <returns>ConfigTableObject</returns>
    [HttpGet("/TablesConfig")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ConfigTableObject>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<ConfigTableObject> GetConfigTables()
    {
        return Ok(_db.GetConfigTables());
    }



    /// <summary>
    /// Queue vacuum to a table
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    [HttpPost("/Vacuum/{table}")]
    public ActionResult<string> Vaccum(string table)
    {
        try
        {
            table = table.Replace(" ", "_").ToLower();
            _db.TableExists(table);
        }
        catch (System.Exception error)
        {
            return BadRequest(error.Message);
        }
        
        _db.VacuumFullTable(table);

        return Ok($"The table '{table}' vacuumed");
    }

    /// <summary>
    /// Queue vacuum full to a table (it will lock the table)
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    [HttpPost("/VacuumFull/{table}")]
    public ActionResult<string> VaccumFull(string table)
    {
        try
        {
            table = table.Replace(" ", "_").ToLower();
            _db.TableExists(table);
        }
        catch (System.Exception error)
        {
            return BadRequest(error.Message);
        }
        
        _db.VacuumFullTable(table);

        return Ok($"The table '{table}' was fully vacuumed");
    }
}

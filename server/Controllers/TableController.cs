using Microsoft.AspNetCore.Mvc;
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
        _db.ValidateTable(table);
        table = table.Replace(" ", "_");
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
}

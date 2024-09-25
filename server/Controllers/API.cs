using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using server.Repositories;

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
        _db.ValidateIndex(index);
        var body = Utils.Base64Replacer.ReplaceBase64Content(obj);
        var json = JsonSerializer.Serialize(body);


        try
        {
            //tenta salvar na tabela do meu index.
            //se der certo, 200
            var id = _db.Insert(index, json);
            return Created("teste", id);
        }
        catch (System.Exception error1)
        {
            //se der erro, verificar que tipo de erro.
            //se a tabela nào existir, entao cria.
            try
            {
                Console.Write($"Creating new table ({index}) ... ");
                _db.CreateIndexTable(index);
                Console.Write("OK! ... ");

                Console.Write($"Creating new JSONB index ({index}) ... ");
                _db.CreateJsonbIndex(index);
                Console.Write("OK! ... ");

                var id = _db.Insert(index, json);
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

    

    

}

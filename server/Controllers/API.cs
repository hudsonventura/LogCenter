using Microsoft.AspNetCore.Mvc;

namespace server.Controllers;

/// <summary>
/// DOC 1
/// </summary>
[ApiController]
public class API : ControllerBase
{


    /// <summary>
    /// DOC2
    /// </summary>
    /// <returns></returns>
    [HttpGet("/{index}/_doc")]
    public ActionResult Index_Doc(string index)
    {
        return Index(index);
    }
    /// <summary>
    /// DOC2
    /// </summary>
    /// <returns></returns>
    [HttpGet("/{index}")]
    public ActionResult Index(string index)
    {
        return Ok(index);
    }
}

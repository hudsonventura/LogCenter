using Microsoft.AspNetCore.Mvc;

namespace server.Controllers;

[Route("[controller]")]
public class TimezonesController : ControllerBase
{

    [HttpGet("/Timezones")]
    public IActionResult Index()
    {
        var timeZones = TimeZoneInfo.GetSystemTimeZones()
            .Select(timeZone => new { timeZone.Id, timeZone.DisplayName })
            .OrderBy(timeZone => timeZone.DisplayName);

        return Ok(timeZones);
    }
}
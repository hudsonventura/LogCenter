using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_example.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;;
    }



    /// <summary>
    /// Test simple 200 Ok
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetWeatherForecast")]
    public ActionResult Get()
    {

        var test = Enumerable.Range(1, 20).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        _logger.LogCritical("Hello World - Info", Guid.NewGuid());


        return Ok(test);

    }


    /// <summary>
    /// Test Badrequest and Post 200 OK
    /// </summary>
    /// <param name="teste"></param>
    /// <returns></returns>
    [HttpPost(Name = "GetWeatherForecast")]
    public ActionResult Get([FromQuery] string teste, [FromBody] string body)
    {

        var test = Enumerable.Range(1, 20).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        _logger.LogCritical("Hello World - Info {aaa}", test);

        return Ok(test);
    }

    /// <summary>
    /// Test Exception
    /// </summary>
    /// <param name="teste"></param>
    /// <returns></returns>
    [HttpPut(Name = "SimulateException")]
    public ActionResult SimulateException()
    {
        throw new Exception("Test Exception");
    }
}

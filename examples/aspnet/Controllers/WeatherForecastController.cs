using System.Diagnostics;
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

    private readonly nuget.ILogger _logger;

    public WeatherForecastController(nuget.ILogger logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        //00-3a30d2b2c73e8a61fceed6060b281b39-5933e5f1b96e134c-00

        var test = Enumerable.Range(1, 20).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        var traceId = HttpContext.Items.FirstOrDefault(x => x.Key == "traceId").Value;

        var testttt = HttpContext;
        
        _logger.LogInformation("Processing ...");
        _logger.LogInformation(test);
        _logger.LogInformation("Ok ...");
        return test;
    }
}

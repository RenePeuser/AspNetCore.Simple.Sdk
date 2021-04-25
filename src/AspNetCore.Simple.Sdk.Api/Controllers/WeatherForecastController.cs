using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Simple.Sdk.Api.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("v{version:apiVersion}/weather")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();



            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = new DateTime(2021, 11, index),
                TemperatureC = 32,
                Summary = Summaries[index]
            })
            .ToArray();
        }
    }
}

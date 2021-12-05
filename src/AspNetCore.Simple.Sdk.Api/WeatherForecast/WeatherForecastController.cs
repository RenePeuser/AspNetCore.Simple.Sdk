using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Simple.Sdk.Api.WeatherForecast
{
    public class SummariesProvider
    {
        public IImmutableList<string> GetAll() => new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" }.ToImmutableList();
    }

    [ApiVersion("1.0")]
    [ApiController]
    [Route("v{version:apiVersion}/weather")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly SummariesProvider _summariesProvider;

        public WeatherForecastController(SummariesProvider summariesProvider)
        {
            _summariesProvider = summariesProvider;
        }


        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = new DateTime(2021, 11, index),
                TemperatureC = 32,
                Summary = _summariesProvider.GetAll()[index]
            }).ToArray();
        }
    }
}

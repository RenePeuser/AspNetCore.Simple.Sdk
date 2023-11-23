using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Simple.Sdk.Api.WeatherForecast.V2
{
    public class WeatherConfig
    {
        public int AmountOfForecasts { get; init; }
        public int Temperature { get; init; }
    }

    public class SummariesProvider
    {
        public IImmutableList<string> AllSummaries { get; } = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" }.ToImmutableList();
    }

    [AllowAnonymous]
    [ApiVersion("2.0")]
    [ApiController]
    [Route("v{version:apiVersion}/weather")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly SummariesProvider _summariesProvider;
        private readonly WeatherConfig _weatherConfig;

        public WeatherForecastController(SummariesProvider summariesProvider, WeatherConfig weatherConfig)
        {
            _summariesProvider = summariesProvider;
            _weatherConfig = weatherConfig;
        }


        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, _weatherConfig.AmountOfForecasts).Select(index => new WeatherForecast
            {
                Date = new DateTime(2021, 11, index),
                TemperatureC = _weatherConfig.Temperature,
                Summary = _summariesProvider.AllSummaries[index]
            }).ToArray();
        }
    }
}

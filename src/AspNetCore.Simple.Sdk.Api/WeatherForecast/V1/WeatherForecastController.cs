using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Simple.Sdk.Api.WeatherForecast.V1
{
    public class WeatherConfig
    {
        public int AmountOfForecasts { get; init; }
        public int Temperature { get; init; }
    }

    public class SummariesProvider
    {
        private static readonly IImmutableList<string> Summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" }.ToImmutableList();

        public IImmutableList<string> GetAllSummaries()
        {
            return Summaries;
        }
    }

    [AllowAnonymous]
    [ApiVersion("1.0")]
    [ApiController]
    [Route("v{version:apiVersion}/weather")]
    public class WeatherForecastController(SummariesProvider summariesProvider,
                                           WeatherConfig weatherConfig) : ControllerBase
    {
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, weatherConfig.AmountOfForecasts).Select(index => new WeatherForecast
            {
                Date = new DateTime(2021, 11, index),
                TemperatureC = weatherConfig.Temperature,
                Summary = summariesProvider.GetAllSummaries()[index]
            });
        }
    }
}

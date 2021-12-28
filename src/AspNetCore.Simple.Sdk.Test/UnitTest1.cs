using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetCore.Simple.MsTest.Sdk;
using AspNetCore.Simple.Sdk.Api.WeatherForecast;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test
{
    [TestClass]
    public class Api_Test : MsTestBase
    {
        [TestMethod]
        public Task Should_Return_Expected_Weather_Result()
        {
            return Client.AssertGetAsync<IEnumerable<WeatherForecast>>("api/test/v1/weather", "[{\"Date\":\"2021-11-01T00:00:00\",\"TemperatureC\":27,\"TemperatureF\":80,\"Summary\":\"Bracing\"},{\"Date\":\"2021-11-02T00:00:00\",\"TemperatureC\":27,\"TemperatureF\":80,\"Summary\":\"Chilly\"},{\"Date\":\"2021-11-03T00:00:00\",\"TemperatureC\":27,\"TemperatureF\":80,\"Summary\":\"Cool\"},{\"Date\":\"2021-11-04T00:00:00\",\"TemperatureC\":27,\"TemperatureF\":80,\"Summary\":\"Mild\"},{\"Date\":\"2021-11-05T00:00:00\",\"TemperatureC\":27,\"TemperatureF\":80,\"Summary\":\"Warm\"}]");
        }
    }
}

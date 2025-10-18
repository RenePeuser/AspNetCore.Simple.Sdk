using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetCore.Simple.MsTest.Sdk;
using AspNetCore.Simple.Sdk.Api.WeatherForecast.V1;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test
{
    [TestClass]
    public class Api_Test : MsTestBase
    {
        [TestMethod]
        public Task Should_Return_Expected_Weather_Result()
        {
            return Client.AssertGetAsync<IEnumerable<WeatherForecast>>("api/sample/v1/weather", /*lang=json,strict*/ "[{\"date\":\"2021-11-01T00:00:00\",\"temperatureC\":27,\"temperatureF\":80,\"summary\":\"Bracing\"},{\"date\":\"2021-11-02T00:00:00\",\"temperatureC\":27,\"temperatureF\":80,\"summary\":\"Chilly\"},{\"date\":\"2021-11-03T00:00:00\",\"temperatureC\":27,\"temperatureF\":80,\"summary\":\"Cool\"},{\"date\":\"2021-11-04T00:00:00\",\"temperatureC\":27,\"temperatureF\":80,\"summary\":\"Mild\"},{\"date\":\"2021-11-05T00:00:00\",\"temperatureC\":27,\"temperatureF\":80,\"summary\":\"Warm\"}]");
        }
    }
}

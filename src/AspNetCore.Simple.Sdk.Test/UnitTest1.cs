using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetCore.Simple.MsTest.Sdk;
using AspNetCore.Simple.Sdk.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test
{
    [TestClass]
    public class UnitTest1 : MsTestBase
    {
        [TestMethod]
        public Task TestMethod1()
        {
            return Client.AssertGetAsync<IEnumerable<WeatherForecast>>("api/test/weather", "[\r\n  {\r\n    \"date\": \"2021-04-26T18:33:30.6365344+02:00\",\r\n    \"temperatureC\": -6,\r\n    \"temperatureF\": 22,\r\n    \"summary\": \"Freezing\"\r\n  },\r\n  {\r\n    \"date\": \"2021-04-27T18:33:30.636988+02:00\",\r\n    \"temperatureC\": 32,\r\n    \"temperatureF\": 89,\r\n    \"summary\": \"Cool\"\r\n  },\r\n  {\r\n    \"date\": \"2021-04-28T18:33:30.6369927+02:00\",\r\n    \"temperatureC\": 25,\r\n    \"temperatureF\": 76,\r\n    \"summary\": \"Balmy\"\r\n  },\r\n  {\r\n    \"date\": \"2021-04-29T18:33:30.6369955+02:00\",\r\n    \"temperatureC\": 44,\r\n    \"temperatureF\": 111,\r\n    \"summary\": \"Freezing\"\r\n  },\r\n  {\r\n    \"date\": \"2021-04-30T18:33:30.6369964+02:00\",\r\n    \"temperatureC\": 28,\r\n    \"temperatureF\": 82,\r\n    \"summary\": \"Cool\"\r\n  }\r\n]");
        }
    }
}

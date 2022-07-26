using System;
using AspNetCore.Simple.Sdk.Api.WeatherForecast;
using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using AspNetCore.Simple.Sdk.Startups;
using AspNetCore.Simple.Sdk.Swagger;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.Api
{
    public class Startup : SimpleStartup
    {
        private static readonly SwaggerInfo _swaggerInfo = new() { Title = "API for test", ContactEmail = "rene.peuser@hotmail.de", ContactName = "Rene Peuser", ContactUrl = new Uri("https://www.rene-peuser-softwaresolutions.de/"), Description = "This is a test application" };

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment) :
            base(configuration, webHostEnvironment, new PathString("/api/test"), _swaggerInfo)
        {
        }

        public override void AutoConfigureServices(AutoRegistration autoRegistration)
        {
            base.AutoConfigureServices(autoRegistration);

            // Hint: Try to use domain root extension to bundle the entry point for a specific domain.
            //       Makes your code maintainable, and nice to read !!
            autoRegistration.AddWeatherForecast();
        }
    }
}

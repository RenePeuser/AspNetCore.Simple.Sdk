using AspNetCore.Simple.Sdk.Api.WeatherForecast;
using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using AspNetCore.Simple.Sdk.Startups;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Api
{
    public class Startup : SimpleStartup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment) : base(configuration, webHostEnvironment, new PathString("/api/test"), "API for test")
        {
        }

        public override void AutoConfigureDevelopmentServices(AutoRegistration autoRegistration)
        {
            base.AutoConfigureDevelopmentServices(autoRegistration);
        }

        public override void AutoConfigureServices(AutoRegistration autoRegistration)
        {
            base.AutoConfigureServices(autoRegistration);

            autoRegistration.DoAutoRegistrationFor<SummariesProvider>();
        }
    }
}

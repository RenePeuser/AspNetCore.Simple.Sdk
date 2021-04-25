using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using AspNetCore.Simple.Sdk.Startups;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.Api
{
    public class Startup : SimpleStartup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment) : base(configuration, webHostEnvironment, new PathString("/api/test"), "API for test")
        {
        }
    }
}

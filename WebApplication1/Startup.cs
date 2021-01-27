using Microsoft.AspNetCore.Hosting;
using AspNetCore.Simple.Sdk.Startups;
using Microsoft.Extensions.Configuration;

namespace WebApplication1
{
    public class Startup : SimpleStartup
    {
        public Startup(IConfiguration configuration,
                       IWebHostEnvironment webHostEnvironment) : base(configuration, webHostEnvironment, typeof(Startup).Assembly, "/api/new", "New API")
        {
        }
    }
}

using System;
using System.IO;
using AspNetCore.Simple.Sdk.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace AspNetCore.Simple.Sdk.Test
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var testAppsettingsJson = new FileInfo(Path.Combine(Environment.CurrentDirectory, "appsettings.test.json"));

            builder.ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.AddJsonFile(testAppsettingsJson.FullName));
            builder.ConfigureServices(services =>
            {
                // if we need to switch between services we have to do it here
            });
        }
    }
}

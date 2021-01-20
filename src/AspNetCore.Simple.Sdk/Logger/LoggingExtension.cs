using Extensions.Pack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger
{
    public static class LoggingExtension
    {
        public static void AddLoggingForPulse(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            services.AddLogging(config =>
            {
                // if we are not in development we have to clean all providers !!
                if (webHostEnvironment.IsDevelopment().IsFalse())
                {
                    // very important clear all other log providers ! currently on survey backend there will be logged to console and many more !!!
                    config.ClearProviders();
                    config.SetMinimumLevel(LogLevel.Warning);
                }
                else
                {
                    config.SetMinimumLevel(LogLevel.Debug);
                }

                var awsLoggingConfigSection = configuration.GetAWSLoggingConfigSection();
                // ToDo: just fast quick hack because we have to move to master.
                var tenant = configuration["Tenant"];
                awsLoggingConfigSection.Config.DisableLogGroupCreation = true;
                awsLoggingConfigSection.Config.LogGroup = awsLoggingConfigSection.Config.LogGroup.Replace("{tenant}", tenant);
                config.AddAWSProvider(awsLoggingConfigSection);
                config.SetMinimumLevel(LogLevel.Information);
            });
        }
    }
}
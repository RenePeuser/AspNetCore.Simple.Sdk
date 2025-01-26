using System;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApplicationInsight.TelemetryProcessors
{
    internal static class AddTraceTelemetryFilterExtension
    {
        public static void AddTraceTelemetryFilter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsSettings(configuration);

            services.AddApplicationInsightsTelemetryProcessor<TraceTelemetryFilter>();
        }
    }

    public record TraceTelemetryFilterSettings
    {
        public string[] NamesToIgnore { get; init; } = Array.Empty<string>();
    }

    internal sealed class TraceTelemetryFilter(ITelemetryProcessor telemetryProcessor,
                                               TraceTelemetryFilterSettings traceTelemetryFilterSettings) : ITelemetryProcessor
    {
        // next will point to the next TelemetryProcessor in the chain.

        public void Process(ITelemetry item)
        {
            if (item is TraceTelemetry traceTelemetry)
            {
                if (traceTelemetryFilterSettings.NamesToIgnore.Any(name => traceTelemetry.Message.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                if (traceTelemetry.Properties.TryGetValue("RequestPath", out var requestPath))
                {
                    if (traceTelemetryFilterSettings.NamesToIgnore.Any(name => requestPath.Contains(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return;
                    }
                }
            }

            telemetryProcessor.Process(item);
        }
    }
}

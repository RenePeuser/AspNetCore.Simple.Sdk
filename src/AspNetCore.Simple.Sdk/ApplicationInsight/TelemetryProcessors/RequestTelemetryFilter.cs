using System;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApplicationInsight.TelemetryProcessors
{
    internal static class AddRequestTelemetryFilterExtension
    {
        public static void AddRequestTelemetryFilter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsSettings(configuration);

            services.AddApplicationInsightsTelemetryProcessor<RequestTelemetryFilter>();
        }
    }

    public record RequestTelemetryFilterSettings
    {
        public string[] NamesToIgnore { get; init; } = Array.Empty<string>();
    }

    internal sealed class RequestTelemetryFilter(ITelemetryProcessor telemetryProcessor,
                                                 RequestTelemetryFilterSettings requestTelemetryFilterSettings) : ITelemetryProcessor
    {
        // next will point to the next TelemetryProcessor in the chain.

        public void Process(ITelemetry item)
        {
            if (item is RequestTelemetry requestTelemetry)
            {
                if (requestTelemetryFilterSettings.NamesToIgnore.Any(name => requestTelemetry.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
            }

            telemetryProcessor.Process(item);
        }
    }
}

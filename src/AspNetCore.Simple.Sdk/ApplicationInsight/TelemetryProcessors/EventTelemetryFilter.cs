using System;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApplicationInsight.TelemetryProcessors
{
    internal static class AddEventTelemetryFilterExtension
    {
        public static void AddEventTelemetryFilter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsSettings(configuration);

            services.AddApplicationInsightsTelemetryProcessor<EventTelemetryFilter>();
        }
    }

    public record EventTelemetryFilterSettings
    {
        public string[] NamesToIgnore { get; init; } = Array.Empty<string>();
    }

    internal sealed class EventTelemetryFilter(ITelemetryProcessor telemetryProcessor,
                                               EventTelemetryFilterSettings eventTelemetrySettings) : ITelemetryProcessor
    {
        // next will point to the next TelemetryProcessor in the chain.

        public void Process(ITelemetry item)
        {
            if (item is EventTelemetry eventTelemetry)
            {
                if (eventTelemetrySettings.NamesToIgnore.Any(name => eventTelemetry.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
            }

            telemetryProcessor.Process(item);
        }
    }
}

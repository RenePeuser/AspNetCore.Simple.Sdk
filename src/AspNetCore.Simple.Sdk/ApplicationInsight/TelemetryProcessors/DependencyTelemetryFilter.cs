using System;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApplicationInsight.TelemetryProcessors
{
    internal static class AddDependencyTelemetryFilterExtension
    {
        public static void AddDependencyTelemetryFilter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsSettings(configuration);

            services.AddApplicationInsightsTelemetryProcessor<DependencyTelemetryFilter>();
        }
    }

    public record DependencyTelemetryFilterSettings
    {
        public string[] NamesToIgnore { get; init; } = Array.Empty<string>();
    }

    internal sealed class DependencyTelemetryFilter : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _telemetryProcessor;
        private readonly DependencyTelemetryFilterSettings _eventTelemetrySettings;

        // next will point to the next TelemetryProcessor in the chain.
        public DependencyTelemetryFilter(ITelemetryProcessor telemetryProcessor, DependencyTelemetryFilterSettings eventTelemetrySettings)
        {
            _telemetryProcessor = telemetryProcessor;
            _eventTelemetrySettings = eventTelemetrySettings;
        }

        public void Process(ITelemetry item)
        {
            if (item is DependencyTelemetry dependencyTelemetry)
            {
                if (_eventTelemetrySettings.NamesToIgnore.Any(name => dependencyTelemetry.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
            }

            _telemetryProcessor.Process(item);
        }
    }
}

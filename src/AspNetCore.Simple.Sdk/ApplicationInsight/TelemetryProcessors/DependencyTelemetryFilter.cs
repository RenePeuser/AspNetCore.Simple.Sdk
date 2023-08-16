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
        private readonly DependencyTelemetryFilterSettings _dependencyTelemetryFilterSettings;

        // next will point to the next TelemetryProcessor in the chain.
        public DependencyTelemetryFilter(ITelemetryProcessor telemetryProcessor,
                                         DependencyTelemetryFilterSettings dependencyTelemetryFilterSettings)
        {
            _telemetryProcessor = telemetryProcessor;
            _dependencyTelemetryFilterSettings = dependencyTelemetryFilterSettings;
        }

        public void Process(ITelemetry item)
        {
            if (item is DependencyTelemetry dependencyTelemetry)
            {
                if (_dependencyTelemetryFilterSettings.NamesToIgnore.Any(name => dependencyTelemetry.Name.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                                                                                 dependencyTelemetry.Data.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    // We stop telemetry processing cause we want to filter out unexpected once.
                    return;
                }
            }

            _telemetryProcessor.Process(item);
        }
    }
}

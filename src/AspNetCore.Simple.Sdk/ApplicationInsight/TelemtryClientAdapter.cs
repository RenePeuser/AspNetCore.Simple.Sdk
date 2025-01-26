using System;
using System.Diagnostics;
using System.Linq;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.ApplicationInsight
{
    public static class AddTelemetryClientAdapterExtension
    {
        public static void AddTelemetryClientAdapter(this IServiceCollection services, IConfiguration configuration)
        {
            if (services.IsAlreadyRegistered<ITelemetryClientAdapter>())
            {
                return;
            }


            // If no ApplicationInsightsSettings available, then activate logger :) 
            if (configuration.TryGetSettings<ApplicationInsightsSettings>(out _).IsFalse())
            {
                services.AddSingletonIfNotExists<ITelemetryClientAdapter, NoTelemetryAdapter>();
                return;
            }

            services.AddTelemetryClient(configuration);

            services.AddSingletonIfNotExists<ITelemetryClientAdapter, TelemetryClientAdapter>();
        }
    }

    public class TelemetryClientAdapter(TelemetryClient telemetryClient) : ITelemetryClientAdapter
    {
        public void TrackInformation(string message, params (string key, string? value)[] details)
        {
            telemetryClient.TrackTrace(message, SeverityLevel.Information, details.ToDictionary());
        }

        public void TrackError(string message, params (string key, string? value)[] details)
        {
            telemetryClient.TrackTrace(message, SeverityLevel.Error, details.ToDictionary());
        }

        public void TrackException(string message, params (string key, string? value)[] details)
        {
#pragma warning disable CA2201 // We just full fill the signature, we do not raise here
            telemetryClient.TrackException(new Exception(message), details.ToDictionary(item => item.key, item => item.value));
#pragma warning restore CA2201 // We just full fill the signature, we do not raise here
        }

        public void TrackException(string message, Exception exception, params (string key, string? value)[] details)
        {
            var valueTuples = ("Exception Message", exception.Message).ToIList();
            var concatedDetails = details.ToList()!.Concat(valueTuples).ToDictionary(item => item.Item1, item => item.Item2);

            telemetryClient.TrackException(exception, concatedDetails!);
        }

        public void TrackException(Exception exception, params (string key, string? value)[] details)
        {
            telemetryClient.TrackException(exception, details.ToDictionary());
        }

        public void TrackTrace(string message, params (string key, string? value)[] details)
        {
            telemetryClient.TrackTrace(message, details.ToDictionary());
        }

        public IOperationHolder<T> StartOperation<T>(Activity activity) where T : OperationTelemetry, new()
        {
            return telemetryClient.StartOperation<T>(activity);
        }

        public void TrackEvent(string eventName, params (string key, string? value)[] details)
        {
            telemetryClient.TrackEvent(eventName, details.ToDictionary());
        }

        public void TrackMetric(string name, double value, params (string key, string? value)[] details)
        {
            telemetryClient.TrackMetric(name, value, details.ToDictionary());
        }

        public void TrackEvent(EventTelemetry eventTelemetry)
        {
            telemetryClient.TrackEvent(eventTelemetry);
        }
    }

    public interface ITelemetryClientAdapter
    {
        void TrackInformation(string message, params (string key, string? value)[] details);
        void TrackError(string message, params (string key, string? value)[] details);
        void TrackException(string message, params (string key, string? value)[] details);
        void TrackException(string message, Exception exception, params (string key, string? value)[] details);
        void TrackException(Exception exception, params (string key, string? value)[] details);
        void TrackTrace(string message, params (string key, string? value)[] details);
        IOperationHolder<T> StartOperation<T>(Activity activity) where T : OperationTelemetry, new();
        void TrackEvent(string eventName, params (string key, string? value)[] details);
        void TrackMetric(string name, double value, params (string key, string? value)[] details);
        void TrackEvent(EventTelemetry eventTelemetry);
    }

    internal sealed class NoTelemetryAdapter(ILogger<NoTelemetryAdapter> logger) : ITelemetryClientAdapter
    {
        public void TrackInformation(string message, params (string key, string? value)[] details)
        {
            logger.LogInformation(message, details);
        }

        public void TrackError(string message, params (string key, string? value)[] details)
        {
            logger.LogError(message, details);
        }

        public void TrackException(string message, params (string key, string? value)[] details)
        {
            logger.LogError(message, details);
        }

        public void TrackException(string message, Exception exception, params (string key, string? value)[] details)
        {
            logger.LogError(message, details);
        }

        public void TrackException(Exception exception, params (string key, string? value)[] details)
        {
            logger.LogError(exception.Message, details);
        }

        public void TrackTrace(string message, params (string key, string? value)[] details)
        {
            logger.LogInformation(message, details);
        }

        public IOperationHolder<T> StartOperation<T>(Activity activity) where T : OperationTelemetry, new()
        {
            return new NoOperation<T>();
        }

        public void TrackEvent(string eventName, params (string key, string? value)[] details)
        {
            logger.LogInformation(eventName, details);
        }

        public void TrackMetric(string name, double value, params (string key, string? value)[] details)
        {
            logger.LogInformation(name, details);
        }

        public void TrackEvent(EventTelemetry eventTelemetry)
        {
            logger.LogInformation(eventTelemetry.Name);
        }
    }


    public record NoOperation<T> : IOperationHolder<T>
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public T Telemetry { get; init; } = default!;
    }
}

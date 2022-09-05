using System;
using System.Diagnostics;
using System.Linq;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApplicationInsight
{
    public static class AddTelemetryClientAdapterExtension
    {
        public static void AddTelemetryClientAdapter(this IServiceCollection services)
        {
            services.AddTelemetryClient();

            services.AddSingletonIfNotExists<ITelemetryClientAdapter, TelemetryClientAdapter>();
        }
    }

    public class TelemetryClientAdapter : ITelemetryClientAdapter
    {
        private readonly TelemetryClient _telemetryClient;

        public TelemetryClientAdapter(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void TrackInformation(string message, params (string key, string? value)[] details)
        {
            _telemetryClient.TrackTrace(message, SeverityLevel.Information, details.ToDictionary());
        }

        public void TrackError(string message, params (string key, string? value)[] details)
        {
            _telemetryClient.TrackTrace(message, SeverityLevel.Error, details.ToDictionary());
        }

        public void TrackException(string message, params (string key, string? value)[] details)
        {
            _telemetryClient.TrackException(new Exception(message), details.ToDictionary(item => item.key, item => item.value));
        }

        public void TrackException(string message, Exception exception, params (string key, string? value)[] details)
        {
            var valueTuples = ("Exception Message", exception.Message).ToIList();
            var concatedDetails = details.ToList()!.Concat(valueTuples).ToDictionary(item => item.Item1, item => item.Item2);

            _telemetryClient.TrackException(exception, concatedDetails!);
        }

        public void TrackException(Exception exception, params (string key, string? value)[] details)
        {
            _telemetryClient.TrackException(exception, details.ToDictionary());
        }

        public void TrackTrace(string message, params (string key, string? value)[] details)
        {
            _telemetryClient.TrackTrace(message, details.ToDictionary());
        }

        public IOperationHolder<T> StartOperation<T>(Activity activity) where T : OperationTelemetry, new()
        {
            return _telemetryClient.StartOperation<T>(activity);
        }

        public void TrackEvent(string eventName, params (string key, string? value)[] details)
        {
            _telemetryClient.TrackEvent(eventName, details.ToDictionary());
        }

        public void TrackMetric(string name, double value, params (string key, string? value)[] details)
        {
            _telemetryClient.TrackMetric(name, value, details.ToDictionary());
        }

        public void TrackEvent(EventTelemetry eventTelemetry)
        {
            _telemetryClient.TrackEvent(eventTelemetry);
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
}

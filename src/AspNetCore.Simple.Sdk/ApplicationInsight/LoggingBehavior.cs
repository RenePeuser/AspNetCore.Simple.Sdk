using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.MediatR;
using Extensions.Pack;
using MediatR;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApplicationInsight
{
    public static class AddLoggingBehaviorExtension
    {
        public static void AddBetterLoggingBehavior(this IServiceCollection services)
        {
            // If Telemetry client already registered go out.
            if (services.IsAlreadyRegistered<ITelemetryClientAdapter>())
            {
                return;
            }

            services.AddTelemetryClientAdapter();
            services.AddLoggingHelper();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        }
    }

    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly LoggingHelper _loggingHelper;
        private readonly ITelemetryClientAdapter _telemetryClientAdapter;

        public LoggingBehavior(ITelemetryClientAdapter telemetryClientAdapter,
                               LoggingHelper loggingHelper)
        {
            _telemetryClientAdapter = telemetryClientAdapter;
            _loggingHelper = loggingHelper;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var cqrsName = typeof(TRequest).Name;
            var cqrsType = request is ICommand or ICommand<TResponse> ? "Command" : "Query";
            var successful = true;

            var executeEvent = new EventTelemetry($"Executed {cqrsType.ToLower()} {cqrsName}") { Timestamp = DateTimeOffset.UtcNow };
            var timer = new Stopwatch();
            timer.Start();

            try
            {
                var response = await next().ConfigureAwait(false);
                return response;
            }
            catch (Exception)
            {
                successful = false;
                throw;
            }
            finally
            {
                timer.Stop();

                // We do not want to log any get health code, in Azure we can monitor health status without tons of useless logs
                var roundedExecutionTime = Math.Round(timer.Elapsed.TotalMilliseconds);
                var loggingProperties = new Dictionary<string, string>
                    {
                        {"Type", cqrsType}, 
                        {"Name", cqrsName}, 
                        {"Success", successful.ToString()},
                        {"Duration in ms", $"{roundedExecutionTime}"} // TODO: Fix
                    };

                loggingProperties.AddRange(_loggingHelper.GetProperties(request, "Payload_")!);

                executeEvent.Properties.AddRange(loggingProperties);
                _telemetryClientAdapter.TrackEvent(executeEvent);
                _telemetryClientAdapter.TrackMetric($"{cqrsName} execution time", roundedExecutionTime);
            }
        }
    }
}

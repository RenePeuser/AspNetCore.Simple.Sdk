using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public static class AddDefaultExceptionExceptionLogStrategyExtension
    {
        public static void AddDefaultExceptionExceptionLogStrategy(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ISpecificErrorLogStrategy, DefaultExceptionExceptionLogStrategy>();
        }

        public static void RemoveDefaultExceptionExceptionLogStrategy(this IServiceCollection services)
        {
            var defaultExceptionHandlers = services.Where(serviceRegistration => serviceRegistration.ImplementationType == typeof(DefaultExceptionExceptionLogStrategy));
            services.RemoveRange(defaultExceptionHandlers);
        }
    }

    public class DefaultExceptionExceptionLogStrategy : ExceptionLogStrategy<Exception>
    {
        public DefaultExceptionExceptionLogStrategy(IJsonSerializer jsonSerializer, ILogger<DefaultExceptionExceptionLogStrategy> logger) : base(jsonSerializer, logger)
        {
        }

        protected override bool CanHandleException(Exception exception)
        {
            var result = base.CanHandleException(exception);
            return result.IsFalse() || result;
        }

        protected override ErrorLogInfo GetErrorLogFrom(HttpContext httpContext, Exception exception)
        {
            return new ErrorLogInfo(string.Empty,
                exception.Message,
                exception.GetType().Name,
                exception.StackTrace?.Split(Environment.NewLine) ?? Enumerable.Empty<string>(),
                httpContext.Request.GetQueryRequestInfo().ToImmutableDictionary(k => k.key, v => v.value),
                new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
        }
    }
}

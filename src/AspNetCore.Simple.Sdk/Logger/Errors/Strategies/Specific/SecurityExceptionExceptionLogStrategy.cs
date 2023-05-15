using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public static class AddSecurityExceptionExceptionLogStrategyExtension
    {
        public static void AddSecurityExceptionExceptionLogStrategy(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ISpecificErrorLogStrategy, SecurityExceptionExceptionLogStrategy>();
        }
    }

    public class SecurityExceptionExceptionLogStrategy : ExceptionLogStrategy<SecurityProblemException>
    {
        public SecurityExceptionExceptionLogStrategy(IJsonSerializer jsonSerializer, ILogger<SecurityExceptionExceptionLogStrategy> logger) : base(jsonSerializer, logger, "Security")
        {
        }

        protected override ErrorLogInfo GetErrorLogFrom(HttpContext httpContext, SecurityProblemException exception)
        {
            return new ErrorLogInfo(string.Empty,
                                    exception.ProblemDetails.Detail ?? "n.A", exception.ProblemDetails.Title ?? "n.A",
                                    exception.StackTrace?.Split(Environment.NewLine) ?? Enumerable.Empty<string>(),
                                    httpContext.Request.GetQueryRequestInfo().ToImmutableDictionary(k => k.key, v => v.value),
                                    new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
        }
    }
}

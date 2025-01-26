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
    public static class AddProblemDetailsExceptionExceptionLogStrategyExtension
    {
        public static void AddProblemDetailsExceptionExceptionLogStrategy(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ISpecificErrorLogStrategy, ProblemDetailsExceptionExceptionLogStrategy>();
        }
    }

    public class ProblemDetailsExceptionExceptionLogStrategy(IJsonSerializer jsonSerializer,
                                                             ILogger<ProblemDetailsExceptionExceptionLogStrategy> logger) : ExceptionLogStrategy<ProblemDetailsException>(jsonSerializer, logger)
    {
        protected override ErrorLogInfo GetErrorLogFrom(HttpContext httpContext, ProblemDetailsException exception)
        {
            return new ErrorLogInfo(string.Empty,
                exception.ProblemDetails.Detail ?? "n.A",
                exception.ProblemDetails.Title ?? "n.A",
                exception.StackTrace?.Split(Environment.NewLine) ?? Enumerable.Empty<string>(),
                httpContext.Request.GetQueryRequestInfo().ToImmutableDictionary(k => k.key, v => v.value),
                new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()),
                exception.GetType().Name);
        }
    }
}

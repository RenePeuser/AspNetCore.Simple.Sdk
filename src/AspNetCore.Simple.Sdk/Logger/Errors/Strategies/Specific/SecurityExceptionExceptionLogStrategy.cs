using System;
using System.Collections.Immutable;
using System.Linq;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public class SecurityExceptionExceptionLogStrategy : ExceptionLogStrategy<SecurityProblemException>
    {
        public SecurityExceptionExceptionLogStrategy(IJsonSerializer jsonSerializer, ILogger<SecurityExceptionExceptionLogStrategy> logger) : base(jsonSerializer, logger, "Security")
        {
        }

        protected override ErrorLogInfo GetErrorLogFrom(HttpContext httpContext, SecurityProblemException exception)
        {
            return new ErrorLogInfo(exception.ProblemDetails.Detail, exception.ProblemDetails.Title,
                                    exception.StackTrace?.Split(Environment.NewLine) ?? Enumerable.Empty<string>(),
                                    httpContext.Request.GetQueryRequestInfo().ToImmutableDictionary(k => k.key, v => v.value));
        }
    }
}

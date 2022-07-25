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
    public class ProblemDetailsExceptionExceptionLogStrategy : ExceptionLogStrategy<ProblemDetailsException>
    {
        public ProblemDetailsExceptionExceptionLogStrategy(IJsonSerializer jsonSerializer, ILogger<ProblemDetailsExceptionExceptionLogStrategy> logger) : base(jsonSerializer, logger)
        {
        }

        protected override ErrorLogInfo GetErrorLogFrom(HttpContext httpContext, ProblemDetailsException exception)
        {
            return new ErrorLogInfo(exception.ProblemDetails.Details, exception.ProblemDetails.Title,
                                    exception.StackTrace?.Split(Environment.NewLine) ?? Enumerable.Empty<string>(),
                                    httpContext.Request.GetQueryRequestInfo().ToImmutableDictionary(k => k.key, v => v.value));
        }
    }
}

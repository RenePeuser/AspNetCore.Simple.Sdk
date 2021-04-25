using System.Linq;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public class ProblemDetailsExceptionLogStrategy : LogStrategy<ProblemDetailsException>
    {
        public ProblemDetailsExceptionLogStrategy(IJsonSerializer jsonSerializer, ILogger<ProblemDetailsExceptionLogStrategy> logger) : base(jsonSerializer, logger)
        {
        }

        protected override ErrorLogInfo GetErrorLogFrom(HttpContext httpContext, ProblemDetailsException exception)
        {
            return new ErrorLogInfo(exception.ProblemDetails.Details, exception.ProblemDetails.Title, exception.StackTrace?.Split(System.Environment.NewLine),
                httpContext.Request.GetQueryRequestInfo().ToDictionary(k => k.key, v => v.value));
        }
    }
}

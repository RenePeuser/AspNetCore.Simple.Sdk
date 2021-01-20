using System.Linq;
using AspNetCore.Simple.Sdk.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Errors.Strategies.Specific
{
    public class ProblemDetailsExceptionLogStrategy : LogStrategy<ProblemDetailsException>
    {
        public ProblemDetailsExceptionLogStrategy(IJsonSerializer jsonSerializer, ILogger<ProblemDetailsExceptionLogStrategy> logger) : base(jsonSerializer, logger)
        {
        }

        protected override ErrorLogInfo GetErrorLogFrom(HttpContext httpContext, ProblemDetailsException exception)
        {
            return new ErrorLogInfo(httpContext.Request.GetCloudFrontId(), exception.ProblemDetails.Detail, exception.ProblemDetails.Title, exception.StackTrace?.Split(System.Environment.NewLine),
                httpContext.Request.GetQueryRequestInfo().ToDictionary(k => k.key, v => v.value));
        }
    }
}
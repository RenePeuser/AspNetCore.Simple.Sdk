using System.Linq;
using AspNetCore.Simple.Sdk.ErrorHandling.Development;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Errors.Strategies.Specific
{
    public class SecurityExceptionLogStrategy : LogStrategy<SecurityProblemException>
    {
        public SecurityExceptionLogStrategy(IJsonSerializer jsonSerializer, ILogger<SecurityExceptionLogStrategy> logger) : base(jsonSerializer, logger, "Security")
        {
        }

        protected override ErrorLogInfo GetErrorLogFrom(HttpContext httpContext, SecurityProblemException exception)
        {
            return new ErrorLogInfo(exception.ProblemDetails.Detail, exception.ProblemDetails.Title, exception.StackTrace?.Split(System.Environment.NewLine),
                httpContext.Request.GetQueryRequestInfo().ToDictionary(k => k.key, v => v.value));
        }
    }
}

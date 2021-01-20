using System;
using System.Linq;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Errors.Strategies.Specific
{
    public class DefaultExceptionLogStrategy : LogStrategy<Exception>
    {
        public DefaultExceptionLogStrategy(IJsonSerializer jsonSerializer, ILogger<DefaultExceptionLogStrategy> logger) : base(jsonSerializer, logger)
        {
        }

        protected override bool CanHandleException(Exception exception)
        {
            // This default exception is very important, this can handle any exception, focused to that case that no any
            // other exception type was able to handle the current exception
            var result = base.CanHandleException(exception);
            return result.IsFalse() || result;
        }

        protected override ErrorLogInfo GetErrorLogFrom(HttpContext httpContext, Exception exception)
        {
            return new ErrorLogInfo(httpContext.Request.GetCloudFrontId(), exception.Message, exception.GetType().Name, exception.StackTrace?.Split(System.Environment.NewLine),
                httpContext.Request.GetQueryRequestInfo().ToDictionary(k => k.key, v => v.value));
        }
    }
}

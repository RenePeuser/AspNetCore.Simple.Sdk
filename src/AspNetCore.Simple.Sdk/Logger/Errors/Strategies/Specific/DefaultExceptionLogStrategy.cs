using System;
using System.Linq;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public class DefaultExceptionLogStrategy : LogStrategy<Exception>
    {
        public DefaultExceptionLogStrategy(IJsonSerializer jsonSerializer, ILogger<DefaultExceptionLogStrategy> logger) : base(jsonSerializer, logger)
        {
        }

        protected override bool CanHandleException(Exception exception)
        {
            var result = base.CanHandleException(exception);
            return result.IsFalse() || result;
        }

        protected override ErrorLogInfo GetErrorLogFrom(HttpContext httpContext, Exception exception)
        {
            return new ErrorLogInfo(exception.Message, exception.GetType().Name,
                                    exception.StackTrace?.Split(Environment.NewLine) ?? Enumerable.Empty<string>(),
                                    httpContext.Request.GetQueryRequestInfo().ToDictionary(k => k.key, v => v.value));
        }
    }
}

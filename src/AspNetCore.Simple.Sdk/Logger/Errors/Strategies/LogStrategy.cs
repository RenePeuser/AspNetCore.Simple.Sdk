using System;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Errors.Strategies
{
    public abstract class LogStrategy<TException> : LogStrategyBase
    {
        protected LogStrategy(IJsonSerializer jsonSerializer, ILogger logger, string errorType = "Error") : base(jsonSerializer, logger, typeof(TException), errorType)
        {
        }

        protected sealed override ErrorLogInfo GetErrorLogFromInternal(HttpContext httpContext, Exception exception)
        {
            return GetErrorLogFrom(httpContext, exception.Cast<TException>());
        }

        protected abstract ErrorLogInfo GetErrorLogFrom(HttpContext httpContext, TException exception);
    }
}

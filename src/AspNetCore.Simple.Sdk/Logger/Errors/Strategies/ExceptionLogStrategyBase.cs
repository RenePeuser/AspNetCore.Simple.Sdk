using System;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public abstract class ExceptionLogStrategyBase(IJsonSerializer jsonSerializer,
                                                   ILogger logger,
                                                   Type exceptionType,
                                                   string errorType)
        : ISpecificErrorLogStrategy
    {
        public virtual bool HandleException(HttpContext context, Exception exception, bool exceptionAlreadyHandled)
        {
            if (exceptionAlreadyHandled)
            {
                return true;
            }

            if (CanHandleException(exception).IsFalse())
            {
                return false;
            }

            var errorInfo = GetErrorLogFromInternal(context, exception);
            logger.LogError($"[{errorType}] {errorInfo.Title} Details: {jsonSerializer.Serialize(errorInfo)}");
            return true;
        }

        protected virtual bool CanHandleException(Exception exception)
        {
            return exception.GetType().EqualsTo(exceptionType);
        }

        protected abstract ErrorLogInfo GetErrorLogFromInternal(HttpContext httpContext, Exception exception);
    }
}

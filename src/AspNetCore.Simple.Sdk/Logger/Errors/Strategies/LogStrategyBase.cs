using System;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public abstract class LogStrategyBase : ISpecificErrorLogStrategy
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;
        private readonly Type _exceptionType;
        private readonly string _errorType;

        protected LogStrategyBase(IJsonSerializer jsonSerializer,
                                  ILogger logger,
                                  Type exceptionType,
                                  string errorType)
        {
            _jsonSerializer = jsonSerializer;
            _logger = logger;
            _exceptionType = exceptionType;
            _errorType = errorType;
        }

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
            _logger.LogError($"[{_errorType}] {errorInfo.Title} Details: {_jsonSerializer.Serialize(errorInfo)}");
            return true;
        }

        protected virtual bool CanHandleException(Exception exception)
        {
            return exception.GetType().EqualsTo(_exceptionType);
        }

        protected abstract ErrorLogInfo GetErrorLogFromInternal(HttpContext httpContext, Exception exception);
    }
}

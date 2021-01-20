using System;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.Logger.Errors.Strategies.Specific
{
    public interface ISpecificErrorLogStrategy
    {
        bool HandleException(HttpContext context, Exception exception, bool exceptionAlreadyHandled);
    }
}
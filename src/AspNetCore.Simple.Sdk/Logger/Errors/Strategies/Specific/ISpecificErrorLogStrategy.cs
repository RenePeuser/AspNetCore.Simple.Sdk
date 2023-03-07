using System;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public interface ISpecificErrorLogStrategy
    {
        bool HandleException(HttpContext context, Exception exception, bool exceptionAlreadyHandled);
    }
}

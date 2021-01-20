using System;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.Logger.Errors.Strategies
{
    public interface IErrorLogStrategy
    {
        void Handle(HttpContext context, Exception exception);
    }
}
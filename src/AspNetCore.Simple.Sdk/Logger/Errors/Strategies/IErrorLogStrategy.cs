using System;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public interface IErrorLogStrategy
    {
        void Handle(HttpContext context, Exception exception);
    }
}
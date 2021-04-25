using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    internal interface ISpecificErrorHandler
    {
        Task<bool> HandleExceptionAsync(HttpContext context, Exception exception, bool lastResult);
    }
}

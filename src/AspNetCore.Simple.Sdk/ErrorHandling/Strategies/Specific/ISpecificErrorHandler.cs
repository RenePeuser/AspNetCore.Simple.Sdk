using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public interface ISpecificErrorHandler
    {
        Task<bool> HandleExceptionAsync(HttpContext context, Exception exception, bool lastResult);
    }
}

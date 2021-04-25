using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    internal interface IErrorHandlingStrategy
    {
        Task HandleAsync(HttpContext context, Exception exception);
    }
}

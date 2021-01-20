using System;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling.Production
{
    internal class DefaultExceptionHandler
    {
        internal Task HandleAsync(HttpContext context, Exception exception)
        {
            context.Response.Clear();
            context.Response.StatusCode = GetErrorCode(exception).Cast<int>();

            return context.Response.WriteAsync(string.Empty);
        }

        private HttpStatusCode GetErrorCode(Exception exception)
        {
            return exception switch
            {
                AuthenticationException _ => HttpStatusCode.Unauthorized,
                UnauthorizedAccessException _ => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.BadRequest
            };
        }
    }
}

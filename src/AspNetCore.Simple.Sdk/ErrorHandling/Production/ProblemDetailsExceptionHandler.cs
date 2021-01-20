using System.Net;
using System.Threading.Tasks;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling.Production
{
    internal class ProblemDetailsExceptionHandler
    {
        internal Task HandleAsync(HttpContext context, ProblemDetailsException problemDetailsException)
        {
            context.Response.Clear();
            context.Response.StatusCode = GetErrorCode(problemDetailsException.ProblemDetails.Status).Cast<int>();
            return context.Response.WriteAsync(string.Empty);
        }

        private HttpStatusCode GetErrorCode(int? status)
        {
            return status switch
            {
                401 or 403 => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.BadRequest,
            };
        }
    }
}

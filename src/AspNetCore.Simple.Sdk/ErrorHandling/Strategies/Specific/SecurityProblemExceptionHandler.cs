using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    internal class SecurityProblemExceptionHandler : SpecificErrorHandler<SecurityProblemException>
    {
        protected override async Task HandleAsync(HttpContext context, SecurityProblemException exception)
        {
            context.Response.Clear();
            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = exception.ProblemDetails.StatusCode;

            await context.Response.WriteAsJsonAsync(exception.ProblemDetails).ConfigureAwait(false);
        }
    }
}

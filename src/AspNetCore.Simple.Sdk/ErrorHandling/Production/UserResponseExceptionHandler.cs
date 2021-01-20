using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling.Production
{
    internal class UserResponseExceptionHandler
    {
        internal Task HandleAsync(HttpContext context, UserResponseException problemDetailsException)
        {
            context.Response.Clear();
            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = problemDetailsException.UserErrorResponse.StatusCode;
            return context.Response.WriteAsJsonAsync(problemDetailsException.UserErrorResponse);
        }
    }
}

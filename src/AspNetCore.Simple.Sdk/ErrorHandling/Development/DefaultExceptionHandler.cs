using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mime;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Simple.Sdk.ErrorHandling.Development
{
    internal class DefaultExceptionHandler
    {
        internal Task HandleAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;

            // If there is no specific exception error handling then internal server error.
            context.Response.StatusCode = GetErrorCode(exception).Cast<int>();

            var problemDetails = new ProblemDetails
            {
                // Status = context.Response.StatusCode,
                // Title = "Exception handled by the middleware",
                Detail = exception.Message
            };

            var problemDetailsSerialized = JsonSerializer.Serialize(problemDetails);
            return context.Response.WriteAsync(problemDetailsSerialized);
        }

        private HttpStatusCode GetErrorCode(Exception exception)
        {
            return exception switch
            {
                ValidationException _ => HttpStatusCode.BadRequest,
                FormatException _ => HttpStatusCode.BadRequest,
                AuthenticationException _ => HttpStatusCode.Unauthorized,
                UnauthorizedAccessException _ => HttpStatusCode.Unauthorized,
                NotImplementedException _ => HttpStatusCode.NotImplemented,
                _ => HttpStatusCode.InternalServerError
            };
        }
    }
}

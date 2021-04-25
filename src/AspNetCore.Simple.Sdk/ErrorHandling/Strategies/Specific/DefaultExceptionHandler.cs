using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mime;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    internal class DefaultExceptionHandler : SpecificErrorHandler<Exception>
    {
        protected override bool CanHandleException(Exception exception)
        {
            // this is the default, which can handle all, so if no other handled the exception this one will.
            return true;
        }

        protected override Task HandleAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;

            // If there is no specific exception error handling then internal server error.
            context.Response.StatusCode = GetErrorCode(exception).Cast<int>();

            var problemDetails = new PulseProblemDetails($"{exception.GetType().Name} was thrown.", exception.Message, context.Response.StatusCode, ImmutableDictionary<string, object>.Empty);

            var problemDetailsSerialized = JsonSerializer.Serialize(problemDetails);
            return context.Response.WriteAsync(problemDetailsSerialized);
        }

        private HttpStatusCode GetErrorCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => HttpStatusCode.BadRequest,
                FormatException => HttpStatusCode.BadRequest,
                AuthenticationException => HttpStatusCode.Unauthorized,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                NotImplementedException => HttpStatusCode.NotImplemented,
                _ => HttpStatusCode.InternalServerError
            };
        }
    }
}

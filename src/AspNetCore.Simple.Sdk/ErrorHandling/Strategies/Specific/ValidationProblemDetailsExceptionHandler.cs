using System.Net.Mime;
using System.Threading.Tasks;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{

    internal static class AddValidationProblemDetailsExceptionHandlerExtension
    {
        public static void AddValidationProblemDetailsExceptionHandler(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ISpecificErrorHandler, ValidationProblemDetailsExceptionHandler>();
        }
    }

    internal sealed class ValidationProblemDetailsExceptionHandler : SpecificErrorHandler<ValidationProblemDetailsException>
    {
        protected override async Task HandleAsync(HttpContext context, ValidationProblemDetailsException exception)
        {
            context.Response.Clear();
            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = exception.ValidationProblemDetails.Status ?? StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(exception.ValidationProblemDetails).ConfigureAwait(false);
        }
    }
}

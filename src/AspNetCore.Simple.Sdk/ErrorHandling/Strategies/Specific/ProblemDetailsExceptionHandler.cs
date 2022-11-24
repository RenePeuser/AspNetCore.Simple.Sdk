using System.Net.Mime;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    internal static class AddProblemDetailsExceptionHandlerExtension
    {
        public static void AddProblemDetailsExceptionHandler(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ISpecificErrorHandler, ProblemDetailsExceptionHandler>();
        }
    }

    internal sealed class ProblemDetailsExceptionHandler : SpecificErrorHandler<ProblemDetailsException>
    {
        protected override async Task HandleAsync(HttpContext context, ProblemDetailsException exception)
        {
            context.Response.Clear();
            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = exception.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(exception.ProblemDetails).ConfigureAwait(false);
        }
    }
}

using System.Net.Mime;
using System.Threading.Tasks;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public static class AddSecurityProblemExceptionHandlerExtension
    {
        public static void AddSecurityProblemExceptionHandler(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ISpecificErrorHandler, SecurityProblemExceptionHandler>();
        }
    }

    internal sealed class SecurityProblemExceptionHandler : SpecificErrorHandler<SecurityProblemException>
    {
        protected override async Task HandleAsync(HttpContext context, SecurityProblemException exception)
        {
            context.Response.Clear();
            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = exception.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(exception.ProblemDetails).ConfigureAwait(false);
        }
    }
}

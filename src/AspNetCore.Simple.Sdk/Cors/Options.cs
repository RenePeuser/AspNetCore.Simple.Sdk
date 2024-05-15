using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;

namespace AspNetCore.Simple.Sdk.Cors
{
    /// <summary>
    /// Options middleware is a smart part to handle preflight calls
    /// </summary>
    public class OptionsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CorsSettings _corsSettings;

        public OptionsMiddleware(RequestDelegate next, CorsSettings corsSettings)
        {
            _next = next;
            _corsSettings = corsSettings;
        }

        public Task Invoke(HttpContext context)
        {
            return BeginInvoke(context);
        }

        private Task BeginInvoke(HttpContext context)
        {
            if (context.Request.Method == "OPTIONS")
            {
                var controllerActionDescriptor = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (controllerActionDescriptor is null)
                {
                    throw new ProblemDetailsException("Middleware is used in wrong order at startup, or you missing [HttpOptions] at your target route",
                        "Please check error details for more",
                        ("Middleware", $"Please check the order of the {nameof(OptionsMiddleware)}. It must be used before the `app.UseEndPoints(..);`"),
                        ("Endpoint", $"Please check that your endpoint: {controllerActionDescriptor?.ActionName} have attribute set: [HttpOptions({controllerActionDescriptor?.ActionName})]"));
                }

                var allowedMethods = controllerActionDescriptor.MethodInfo.GetCustomAttributes<HttpMethodAttribute>()
                                                               .SelectMany(attribute => attribute.HttpMethods)
                                                               .Select(name => name)
                                                               .ToImmutableList();

                context.Response.Headers.Append("Access-Control-Allow-Origin", _corsSettings.Origins.ToArray());
                context.Response.Headers.Append("Access-Control-Allow-Headers", _corsSettings.Headers.ToArray());
                context.Response.Headers.Append("Access-Control-Allow-Methods", new[] { allowedMethods.Flatten(", ") });
                context.Response.Headers.Append("Access-Control-Allow-Credentials", new[] { _corsSettings.AllowCredentials.ToString() });
                context.Response.StatusCode = 204;
                return Task.CompletedTask;
            }

            return _next.Invoke(context);
        }
    }

    public static class OptionsMiddlewareExtensions
    {
        public static IApplicationBuilder UseOptions(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OptionsMiddleware>();
        }
    }
}

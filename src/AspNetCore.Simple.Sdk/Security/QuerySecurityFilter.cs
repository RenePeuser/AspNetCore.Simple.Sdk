using System.Linq;
using AspNetCore.Simple.Sdk.ErrorHandling.Development;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Security
{
    public static class AddQuerySecurityFilterExtension
    {
        public static void AddQuerySecurityFilter(this IServiceCollection services)
        {
            services.AddMvc(config => config.Filters.Add<QuerySecurityFilter>());
        }
    }

    public class QuerySecurityFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var declaredQueryParameters = context.ActionDescriptor.GetQueryParameters();
            var request = context.HttpContext.Request;
            var requestQueryParameters = request.Query.Keys;

            var names = declaredQueryParameters.Select(d => d.BindingInfo.BinderModelName.IsNotNullOrWhiteSpace() ? d.BindingInfo.BinderModelName : d.Name);

            // we accept only request which match exactly the query params what we have if we have a mismatch we throw directly
            if (requestQueryParameters.Any(param => names.Contains(param).IsFalse()))
            {
                throw new SecurityProblemException("Invalid query parameters", "Possible attack detected", request.GetQueryRequestInfo().ToArray());
            }
        }
    }
}

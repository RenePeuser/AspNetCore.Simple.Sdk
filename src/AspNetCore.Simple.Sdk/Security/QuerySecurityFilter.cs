using System.Collections.Immutable;
using System.Linq;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetCore.Simple.Sdk.Security
{
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
            if (requestQueryParameters.Any(param => names.Contains(param).IsFalse()))
            {
                throw new SecurityProblemException("Invalid query parameters", "Possible attack detected", request.GetQueryRequestInfo().ToImmutableDictionary(item => item.key, item => item.value));
            }
        }
    }
}

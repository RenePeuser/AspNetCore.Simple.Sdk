using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Security
{
    public static class AddQuerySecurityFilterExtension
    {
        public static void AddQuerySecurityFilter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddQuerySecurityFilterSettings(configuration);
            services.AddMvc(config => config.Filters.Add<QuerySecurityFilter>());
        }
    }

    internal sealed class QuerySecurityFilter : IAsyncActionFilter
    {
        private readonly QuerySecurityFilterSettings _querySecurityFilterSettings;

        public QuerySecurityFilter(QuerySecurityFilterSettings querySecurityFilterSettings)
        {
            _querySecurityFilterSettings = querySecurityFilterSettings;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ValidateQueryParams(context);

            await next().ConfigureAwait(false);
        }

        private void ValidateQueryParams(ActionExecutingContext context)
        {
            var declaredQueryParameters = context.ActionDescriptor.GetQueryParameters().ToList();
            var request = context.HttpContext.Request;
            var requestQueryParameters = request.Query.Keys.Except(_querySecurityFilterSettings.QueryParamsToIgnore).ToList();

            var names = declaredQueryParameters.Select(d => d.BindingInfo!.BinderModelName.IsNotNullOrWhiteSpace() ? d.BindingInfo.BinderModelName : d.Name).ToList();

            // we accept only request which match exactly the query params what we have if we have a mismatch we throw directly
            if (requestQueryParameters.Any(param => names.Contains(param).IsFalse()))
            {
                var requestInfo = request.GetQueryRequestInfo().ToArray();
                throw new SecurityProblemException("Invalid query parameters",
                    "Possible attack detected",
                    requestInfo);
            }
        }
    }
}

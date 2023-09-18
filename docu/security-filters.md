## Security filter.
This security filter is just a sample query check, which checks that the request contains no parameters which are not implemented. It is just an emergency check if an attacked reach the API and try any kind of injection calls.

```xml
"QuerySecurityFilterSettings": {
    "QueryParamsToIgnore": "paramX;paramY"
  }
```


```csharp
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
```
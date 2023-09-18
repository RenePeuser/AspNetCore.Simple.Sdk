## ErrorHandling
We currently follow the early exit strategy. So where ever you have an unexpected situation you can fire an `ProblemDetailsException`.
Error response and logging will be done automatically. 

**Benefits**:

* Independent error response infos
* Performance benefit => Because the amount of try catch blocks have to be reduced to a mandatory minimum
* Much more cleaner code
* You has to throw just an exception and all the rest will be handled automatically. (Prefer not to use system exception)
* Better to extend because of using strategies.
* Centralized and unique error responses -> You avoid the user to handle different of error response types !!

## How to use it

Just derived from the `SimpleStartup` class and error handling is completed :)
```csharp
public class Startup : SimpleStartup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment) :
            base(configuration, webHostEnvironment, string.Empty)
    {
    }
}
```

## Sample - Just throw an exception :) 
```csharp
if (user.IsNull())
{
     throw new ProblemDetailsException(HttpStatusCode.NotFound,
     				                   "No user was found",
     				                   $"The user with the Id '{userId}' does not exist",     				                
     				                   ("UserId", userId));
}
```

## Error result json
We follow now [RFC7231](https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1)
```csharp
{
  "title": "No user was found",
  "status": 400,
  "detail": "The user with the Id : \u002715265b9a-9d21-42ee-9950-4d1dc7260cba\u0027 does not exist",
  "userId": "15265b9a-9d21-42ee-9950-4d1dc7260cba",
}
```

## ErrorHandle-Middleware (is already used and activated over the `SimpleStartup`)

```csharp
internal class ErrorHandlingMiddleware : IMiddleware
{
    private readonly IErrorHandlingStrategy _errorHandlingStrategy;
    
    public ErrorHandlingMiddleware(IErrorHandlingStrategy errorHandlingStrategy)
    {
        _errorHandlingStrategy = errorHandlingStrategy;
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
        	await next(context).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
        	await _errorHandlingStrategy.HandleAsync(context, exception).ConfigureAwait(false);
        }
    }
}
```

## Custom error handle strategy (is already used and activated over the `SimpleStartup`)
If you want to implement a specific error handling strategy just derive from `SpecificErrorHandler<T>`
The `<T>` means the type of your thrown exception you wan to handle
```csharp
internal static class AddProblemDetailsExceptionHandlerExtension
{
    public static void AddProblemDetailsExceptionHandler(this IServiceCollection services)
    {
        services.AddSingletonIfNotExists<ISpecificErrorHandler, ProblemDetailsExceptionHandler>();
    }
}


internal class ProblemDetailsExceptionHandler : SpecificErrorHandler<ProblemDetailsException>
{
    protected override async Task HandleAsync(HttpContext context, ProblemDetailsException exception)
    {
         context.Response.Clear();
         context.Response.ContentType = MediaTypeNames.Application.Json;
         context.Response.StatusCode = exception.ProblemDetails.StatusCode;
         
         await context.Response.WriteAsJsonAsync(exception.ProblemDetails).ConfigureAwait(false);
    }
}
```

## ProblemDetailsException
We provide as result the [ProblemDetails](https://source.dot.net/#Microsoft.AspNetCore.Http.Abstractions/ProblemDetails/ProblemDetails.cs,9fba0244fad2ef1b)
```csharp
public class ProblemDetailsException : Exception
{
    public ProblemDetailsException(string title,
				   params (string key, string value)[] extensions) : this(HttpStatusCode.InternalServerError, title, string.Empty, extensions)
    {
    }
    
    public ProblemDetailsException(string title,
                                   string details,
                                   params (string key, string value)[] extensions) : this(HttpStatusCode.InternalServerError, title, details, extensions)
    {
    }
    
    public ProblemDetailsException(HttpStatusCode statusCode,
                                   string title,
                                   string details,
                                   params (string key, string value)[] extensions) : this(statusCode.ToInt(), title, details, extensions.ToImmutableDictionary(item => item.key, item => item.value.ToString()))
    {
    }
    
    public ProblemDetailsException(int statusCode,
                                   string title,
                                   string details,
                                   params (string key, string value)[] extensions) : this(statusCode.ToInt(), title, details, extensions.ToImmutableDictionary(item => item.key, item => item.value.ToString()))
    {
    }
    
    public ProblemDetailsException(int statusCode,
                                   string title,
                                   string details,
                                   IImmutableDictionary<string, string> errorDetails) : base(title)
    {
        var problemDetails = new ProblemDetails()
        {
            Title = title.IsEmpty() ? null : title,
            Detail = details.IsEmpty() ? null : details,
            Status = statusCode
        };
        
        errorDetails.OrderBy(item => item.Key).ForEach(keyValue =>
        {
            var key = keyValue.Key.Split(" ").Select(value => value.FirstCharToUpper()).Flatten().FirstCharToLower();
            problemDetails.Extensions.Add(key, keyValue.Value);
        });
        
        ProblemDetails = problemDetails;
    }
    
    public ProblemDetails ProblemDetails { get; }
}
```
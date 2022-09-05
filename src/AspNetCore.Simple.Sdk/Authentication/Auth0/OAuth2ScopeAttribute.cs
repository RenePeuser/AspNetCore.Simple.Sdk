using System;
using System.Collections.Immutable;
using System.Net;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetCore.Simple.Sdk.Authentication.Auth0
{
    // validOn = AttributeTargets.Method, because:
    // 1) as per the Swagger documentation (https://swagger.io/docs/specification/authentication/oauth2/),
    //    scopes can only be declared on individual operations (or on the API itself, if all operations have exactly the same scope).
    //    But there is no concept of "controller" in Swagger; therefore, we don't need to declare AttributeTargets.Class.
    // 2) Defining granular scopes is recommended by Microsoft, see
    //    https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-verification-scope-app-roles?tabs=aspnetcore#verify-the-scopes-more-globally
    //
    // AllowMultiple = false, since the idea of this current simple version is to apply at most a *single* scope value to each controller and/or action.
    // If *multiple* scope values per route are ever needed, we also have to define and document their logical connectivity (conjuntion && or disjunction |).
    [AttributeUsage(AttributeTargets.Method)]
    public class OAuth2ScopeAttribute : ActionFilterAttribute
    {
        /// <summary>Creates a new <see cref="OAuth2ScopeAttribute"/> instance.</summary>
        /// <param name="scope">The required scope for the operation.</param>
        public OAuth2ScopeAttribute(string scope)
        {
            Scope = scope;
        }

        public string Scope { get; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // If user does not have the scope claim, get out of here
            var scopeClaim = context.HttpContext.User.FindFirst(c => c.Type == "scope");
            if (scopeClaim.IsNull())
            {
                throw new ProblemDetailsException(HttpStatusCode.BadRequest,
                                                  "Token does not contain a scope.",
                                                  "Please ensure your token has an OAuth 2.0 \"scope\" claim defined.",
                                                  ("Request", $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"),
                                                  ("Scope needed", Scope));
            }

            // Split the scope string into an array.
            // The scope string is case-sensitive, and individual values are separated by space. See also IETF
            // RFC 6749 section 3.3. (https://www.rfc-editor.org/rfc/rfc6749#section-3.3), which is referred to by
            // RFC 8693 section 4.2. (https://www.rfc-editor.org/rfc/rfc8693#section-4.2).
            var scopes = scopeClaim.Value.Split(' ').ToImmutableList();
            if (scopes.ContainsAny(Scope))  // uses case-sensitive comparison
            {
                return;
            }

            throw new ProblemDetailsException(HttpStatusCode.Unauthorized,
                                              "Your token does not contain the correct scope to authorizate calls to this route.",
                                              "Please ask your administrator or service to permit access for this request.",
                                              ("Request", $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"),
                                              ("Scope needed", Scope),
                                              ("Scopes from token", scopes.ToJson()));
        }
    }
}

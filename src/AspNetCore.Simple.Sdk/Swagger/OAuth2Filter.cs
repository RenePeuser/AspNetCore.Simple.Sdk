using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AspNetCore.Simple.Sdk.Authentication.Auth0;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    /// <summary>Adds OAuth 2.0 security definitions and requirements. Required scopes are determined by the presence of the
    /// <see cref="OAuth2ScopeAttribute"/> on the controller actions.
    /// </summary>
    public sealed class OAuth2Filter : IDocumentFilter
    {
        private readonly Auth0 _auth0Settings;
        private readonly SwaggerInfos _swaggerInfos;
        private const string TargetOauth2SecuritySchemeName = "oauth2";
        private readonly string[] _pathToIgnore;

        // Ctor must be public for DI, even if the class itself is internal.
        public OAuth2Filter(Auth0 auth0Settings, SwaggerInfos swaggerInfos)
        {
            _auth0Settings = auth0Settings;
            _swaggerInfos = swaggerInfos;
            _pathToIgnore = swaggerInfos.PathToIgnore.Split(";", StringSplitOptions.RemoveEmptyEntries);
        }

        void IDocumentFilter.Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var selectedVersion = swaggerDoc.Info.Version.ToApiVersion();

            // ------------------------------ Prepare the target document's OAuth 2.0 security scheme
            var targetOAuth2Flow = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri(_auth0Settings.TokenEndpoint)
            };

            // targetOAuth2Flow.Scopes will be set later further below, once we know the aggregated set of scopes used by the operations.
            var targetOAuth2SecurityScheme = CreateOpenApiSecurityScheme(TargetOauth2SecuritySchemeName, targetOAuth2Flow);

            // ------------------------------ Get the source code operations (and their OAuth 2.0 scopes)
            var oauth2ScopedSourceOpsPerRelativePath = context
                .ApiDescriptions
                .Where(desc =>
                {
                    var controllerActionDescriptor = desc.ActionDescriptor.As<ControllerActionDescriptor>();
                    if (controllerActionDescriptor.IsNull())
                    {
                        return false;
                    }

                    if (_swaggerInfos.IncludeOnlyVersionedPaths.IsFalse())
                    {
                        return true;
                    }

                    var apiVersionAttribute = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<ApiVersionAttribute>();
                    if (apiVersionAttribute.IsNull())
                    {
                        return false;
                    }

                    return apiVersionAttribute.Versions.Any(version => version == selectedVersion);
                })
                .Select(sourceApi => new Test(sourceApi.RelativePath?.Replace("{version}", swaggerDoc.Info.Version) ?? string.Empty,
                                              sourceApi.HttpMethod,  // null meaning "all HTTP methods"
                                                                     // Applying FirstOrDefault is o.k., because OAuth2ScopeAttribute has AllowMultiple = false.
                                              sourceApi.ActionDescriptor.EndpointMetadata.OfType<OAuth2ScopeAttribute>().FirstOrDefault()?.Scope))

                .Where(sourceOp => sourceOp.OAuth2Scope.IsNotNullOrEmpty())
                .GroupBy(sourceOp => sourceOp.RelativePath.Replace("{version}", swaggerDoc.Info.Version))
                .ToImmutableDictionary(sourceOpGrp => sourceOpGrp.Key, sourceOpGrp => sourceOpGrp.ToImmutableArray());

            // ------------------------------ Copy the source code operation OAuth 2.0 scopes to the target document's operations.
            var distinctTargetOAuth2Scopes = new HashSet<string>();

            foreach (var targetPathEntry in swaggerDoc.Paths)
            {
                if (oauth2ScopedSourceOpsPerRelativePath.TryGetValue(targetPathEntry.Key.TrimStart('/'), out var sourceOps))
                {
                    foreach (var targetOp in targetPathEntry.Value.Operations)
                    {
                        var targetOpHttpMethod = targetOp.Key.ToString();
                        var sourceOp =
                            sourceOps.FirstOrDefault(sourceOp => sourceOp.HttpMethod?.Equals(targetOpHttpMethod, StringComparison.OrdinalIgnoreCase) == true) ??
                            sourceOps.FirstOrDefault(sourceOp => sourceOp.HttpMethod is null);  // null meaning "any HTTP method", as documented by ApiDescription.HttpMethod

                        if (sourceOp?.OAuth2Scope is { } targetOpOAuth2Scope)
                        {
                            targetOp.Value.Security.Add(new OpenApiSecurityRequirement { [targetOAuth2SecurityScheme] = ImmutableArray.Create(targetOpOAuth2Scope) });
                            distinctTargetOAuth2Scopes.Add(targetOpOAuth2Scope);  // update the overall set of scopes
                        }
                    }
                }
            }

            // ------------------------------ Copy the final set of source code OAuth 2.0 scopes to the target document's OAuth 2.0 security scheme
            targetOAuth2Flow.Scopes = distinctTargetOAuth2Scopes.ToDictionary(
                targetOAuth2Scope => targetOAuth2Scope,
                targetOAuth2Scope => $"The {targetOAuth2Scope} scope.");

            // ------------------------------ Add or replace the target document's OAuth 2.0 security scheme
            swaggerDoc.Components.SecuritySchemes[TargetOauth2SecuritySchemeName] = targetOAuth2SecurityScheme;

            // ------------------------------ Only if needed: Add the Oauth 2.0 security scheme to the target document's root security declaration
            // The target document's root security declaration
            //
            // - *can* be used (optionally) to declare the "one and only" scope of the API, if all operations of the API have exactly the same scope.
            //   We don not implement this option here, since we have already declared (above) the scope on each individual operation, even if it's always the same.
            //   (And it would be illegal to add scopes both on the operations and on the root security.)
            //   See also the Swagger OAuth 2.0 documentation (https://swagger.io/docs/specification/authentication/oauth2/), section "About Scopes".
            //
            // - *must* be used to declare that the API has no scopes at all, if none of the operations has any scope.
            //   See also the Swagger OAuth 2.0 documentation (https://swagger.io/docs/specification/authentication/oauth2/), section "No Scopes".
            //   This is implemented in the following line of code.
            if (distinctTargetOAuth2Scopes.Count == 0) { swaggerDoc.SecurityRequirements.Add(new() { [targetOAuth2SecurityScheme] = ImmutableArray<string>.Empty }); }
        }

        internal sealed record Test(string RelativePath, string? HttpMethod, string? OAuth2Scope);

        private static OpenApiSecurityScheme CreateOpenApiSecurityScheme(string targetOauth2SecuritySchemeName, OpenApiOAuthFlow targetOAuth2Flow)
        {
            return new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Name = targetOauth2SecuritySchemeName,
                Reference = new()
                {
                    Type = ReferenceType.SecurityScheme,
                    // The ID is needed by OpenApiSecuritySchemeReferenceEqualityComparer, which is used by OpenApiSecurityRequirement,
                    // which derives from Dictionary<OpenApiSecurityScheme, IList<String>>.
                    Id = targetOauth2SecuritySchemeName
                },
                Flows = new()
                {
                    // An API does not really care about the kind of flow. However, OpenAPI forces us to choose a flow.
                    // --> We arbitrarily choose the "ClientCredentials" flow to declare the token URL and scpes within.
                    ClientCredentials = targetOAuth2Flow
                }
            };
        }

        //private IImmutableList<ApiDescription> GetAllVersionDependentApiDescriptions(OpenApiDocument swaggerDoc, DocumentFilterContext documentFilterContext)
        //{
        //    var selectedVersion = swaggerDoc.Info.Version.ToApiVersion();

        //    var oauth2ScopedSourceOpsPerRelativePath = documentFilterContext
        //                                               .ApiDescriptions
        //                                               .Where(desc =>
        //                                               {
        //                                                   // 1. Must be an controller action descriptor otherwise no checks can be done
        //                                                   var controllerActionDescriptor = desc.ActionDescriptor.As<ControllerActionDescriptor>();
        //                                                   if (controllerActionDescriptor.IsNull())
        //                                                   {
        //                                                       return false;
        //                                                   }

        //                                                   // 2. Detect all path exists on the controller to check path to ignore
        //                                                   var allPaths = GetAllPaths(controllerActionDescriptor).ToImmutableList();
        //                                                   if (_pathToIgnore.Any(pathToIgnore => allPaths.Any(path => path.Contains(pathToIgnore))))
        //                                                   {
        //                                                       return false;
        //                                                   }

        //                                                   // 3. Now we have to check if version attribute exi
        //                                                   var apiVersionAttribute = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<ApiVersionAttribute>();
        //                                                   if (apiVersionAttribute.IsNull())
        //                                                   {
        //                                                       return false;
        //                                                   }

        //                                                   // 4. Then we have to check if the current selected version fits the controller, because we only want to show version
        //                                                   //    specific controller, routes, tags and schemas
        //                                                   if (apiVersionAttribute.Versions.Any(version => version == selectedVersion).IsFalse())
        //                                                   {
        //                                                       return false;
        //                                                   }

        //                                                   // 5. If all versions and non versions allowed we return true to show all
        //                                                   if (_swaggerInfos.IncludeOnlyVersionedPaths.IsFalse())
        //                                                   {
        //                                                       return true;
        //                                                   }

        //                                                   // 6. If only version path are allowed we have to check if version placeholder exists
        //                                                   if (allPaths.All(path => path.Contains("{version}").IsFalse()))
        //                                                   {
        //                                                       return false;
        //                                                   }

        //                                                   return false;
        //                                               }).ToImmutableList();

        //    return oauth2ScopedSourceOpsPerRelativePath;
        //}

        private IEnumerable<string> GetAllPaths(ControllerActionDescriptor controllerActionDescriptor)
        {
            var controllerBasePaths = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes<RouteAttribute>();
            foreach (var controllerBasePath in controllerBasePaths)
            {
                yield return controllerBasePath.Template;
            }

            var controllerMethods = controllerActionDescriptor.ControllerTypeInfo.GetMethods();
            var methodRouteAttributes = controllerMethods.SelectMany(method => method.GetCustomAttributes<RouteAttribute>());
            foreach (var methodRouteAttribute in methodRouteAttributes)
            {
                yield return methodRouteAttribute.Template;
            }

            var httpAttributes = controllerMethods.SelectMany(method => method.GetCustomAttributes<HttpMethodAttribute>());
            foreach (var httpMethodAttribute in httpAttributes)
            {
                if (httpMethodAttribute.Template.IsNotNull())
                {
                    yield return httpMethodAttribute.Template;
                }
            }
        }
    }
}


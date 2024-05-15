using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public sealed class ReplaceVersionWithExactValueInPathFilter : IDocumentFilter
    {
        private readonly SwaggerInfos _swaggerInfos;

        public ReplaceVersionWithExactValueInPathFilter(SwaggerInfos swaggerInfos)
        {
            _swaggerInfos = swaggerInfos;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Amazing hack, thanks that swashbuckle is not able to detect unique paths absolutely amazing
            SwaggerUi.SelectedVersion = context.DocumentName.ToApiVersion();

            var collectPathInfos = _swaggerInfos.IncludeOnlyVersionedPaths
                ? CollectInfosVersionOnly(swaggerDoc, context, SwaggerUi.SelectedVersion).Distinct(item => item.key).ToImmutableList()
                : CollectInfos(swaggerDoc, context, SwaggerUi.SelectedVersion).Distinct(item => item.key).ToImmutableList();

            var newPath = new OpenApiPaths();
            collectPathInfos.ForEach(path => newPath.Add(path.key, path.openApiPathItem));
            swaggerDoc.Paths = newPath;
        }


        private IEnumerable<(string key, OpenApiPathItem openApiPathItem)> CollectInfosVersionOnly(OpenApiDocument swaggerDoc, DocumentFilterContext context, ApiVersion selectedApiVersion)
        {
            foreach (var path in swaggerDoc.Paths)
            {
                // Include only version path = true means only path with versions
                if (_swaggerInfos.IncludeOnlyVersionedPaths && path.ToString().DoesNotContain("{version}"))
                {
                    continue;
                }

                // filter all out which are not needed for current version
                var apiDescriptions = context.ApiDescriptions.Where(api => api.RelativePath.EqualsTo(path.Key.TrimStart('/'))).ToImmutableList();
                var httpMethods = GetAllVersionAndGroupSpecific(apiDescriptions, selectedApiVersion);

                foreach (var apiDescription in apiDescriptions)
                {
                    var controller = apiDescription.ActionDescriptor.As<ControllerActionDescriptor>();
                    if (controller.IsNull())
                    {
                        throw new ProblemDetailsException("Unexpected ApiDescription type");
                    }

                    var apiVersion = controller.ControllerTypeInfo.GetCustomAttribute<ApiVersionAttribute>();
                    if (apiVersion.IsNull() && _swaggerInfos.IncludeOnlyVersionedPaths)
                    {
                        continue;
                    }

                    if (apiVersion?.Versions[0] != selectedApiVersion)
                    {
                        continue;
                    }

                    var versionInfo = apiDescription.ActionDescriptor.EndpointMetadata.FirstOrDefaultOfType<ApiVersionAttribute>();

                    httpMethods = httpMethods.Select(item => item with { Route = item.Route.Replace("//", "/") }).ToImmutableList();

                    // New feature if path without version should be ignored we do not list it any more
                    if (versionInfo.IsNull() && _swaggerInfos.IncludeOnlyVersionedPaths)
                    {
                        continue;
                    }

                    var newOpenApiPathItem = path.Value;
                    // Now we have to check if the real Http Action with the route is in the list

                    var methodsRealExists = httpMethods.Where(httpMethod => httpMethod.Route.Trim('/').EqualsTo(path.Key.Trim('/'))).ToList();

                    //// Remove those operations which the controller does not have
                    //// This is a evil part when using versioned swagger documents :/
                    var operationToRemove = newOpenApiPathItem.Operations.Where(item => methodsRealExists.Any(m => m.HttpMethod.ToUpperInvariant().EqualsTo(item.Key.ToInvariantString().ToUpperInvariant())).IsFalse()).ToImmutableList();
                    newOpenApiPathItem.Operations.RemoveRange(operationToRemove);


                    if (versionInfo.IsNull())
                    {
                        yield return (path.Key.Replace("{version}", swaggerDoc.Info.Version), newOpenApiPathItem);
                        continue;
                    }

                    if (versionInfo.Versions.Any(v => v.EqualsTo(selectedApiVersion)))
                    {
                        yield return (path.Key.Replace("{version}", swaggerDoc.Info.Version), newOpenApiPathItem);
                    }
                }
            }
        }

        private IImmutableList<HttpMethodInfo> GetAllVersionAndGroupSpecific(ImmutableList<ApiDescription> apiDescriptions,
                                                                             ApiVersion selectedApiVersion)
        {
            // 1. Same group is important !
            var versionBasedApiDescriptions = GetAllApiDescriptionsForSelectedVersion(apiDescriptions, selectedApiVersion).ToImmutableList();
            var sameGroup = versionBasedApiDescriptions.Where(apiDescription =>
            {
                if (apiDescriptions[0].GroupName.IsNull())
                {
                    return true;
                }

                return apiDescription.GroupName == apiDescriptions[0].GroupName;
            }).ToImmutableList();

            // 2. Get controller infos
            var controllers = sameGroup.Select(api => api.ActionDescriptor.As<ControllerActionDescriptor>()).FilterNullObjects().ToImmutableList();

            // 3. Get those which are match to the selected version
            var sameVersions = controllers.Where(c =>
            {
                var versionAttribute = c.ControllerTypeInfo.GetCustomAttribute<ApiVersionAttribute>();
                if (versionAttribute.IsNull())
                {
                    return false;
                }

                return versionAttribute.Versions.Any(v => v.EqualsTo(selectedApiVersion));
            });


            var result = sameVersions.SelectMany(controller =>
            {
                var baseRoutes = controller.ControllerTypeInfo.GetCustomAttributes<RouteAttribute>();
                var baseRouteLikeSwaggerPrepared = baseRoutes.Select(item => item.Template.Replace("v{version:apiVersion}", "/v{version}")).ToList();

                var httpMethods = controller.ControllerTypeInfo.DeclaredMethods.SelectMany(method =>
                {
                    var httpMethods = method.GetCustomAttributes<HttpMethodAttribute>();

                    // first method only stuff
                    var paths = httpMethods.Select(httpMethod =>
                    {
                        var subRouteByHttpAction = httpMethod.Template.IsNotNull() ? $"/{httpMethod.Template}" : string.Empty;
                        return new HttpMethodInfo(httpMethod.HttpMethods.First(), subRouteByHttpAction);
                    }).ToList();

                    // check if route attribute is there
                    var routeAttributes = method.GetCustomAttributes<RouteAttribute>() ?? new List<RouteAttribute>();
                    if (routeAttributes.IsEmpty())
                    {
                        var pathWitoutRoutes = baseRouteLikeSwaggerPrepared.SelectMany(item => paths.Select(p => p with { Route = $"{item}/{p.Route}".Replace("//", "/").Replace("*", string.Empty).TrimEnd('/') }).ToList());
                        return pathWitoutRoutes;
                    }

                    var routePaths = routeAttributes.Select(attribute => attribute.Template);

                    var realPaths = routePaths.SelectMany(route =>
                    {
                        return paths.Select(p => new HttpMethodInfo(p.HttpMethod, $"{route}/{p.Route}"));
                    }).ToList();


                    var absolutePath = baseRouteLikeSwaggerPrepared.SelectMany(item => realPaths.Select(p => p with { Route = $"{item}/{p.Route}".TrimEnd('/') })).ToList();
                    var trimSpecialCases = absolutePath.Select(p => p with { Route = p.Route.Replace("//", "/").Replace("*", string.Empty).TrimEnd('/') }).ToList();
                    return trimSpecialCases;
                }).ToList();

                return httpMethods;
            }).ToImmutableList();

            return result;
        }

        private IEnumerable<ApiDescription> GetAllApiDescriptionsForSelectedVersion(ImmutableList<ApiDescription> apiDescriptions,
                                                                                    ApiVersion selectedApiVersion)
        {
            foreach (var apiDescription in apiDescriptions)
            {
                var controller = apiDescription.ActionDescriptor.As<ControllerActionDescriptor>();
                if (controller.IsNull())
                {
                    throw new ProblemDetailsException("Unexpected ApiDescription type");
                }

                var apiVersion = controller.ControllerTypeInfo.GetCustomAttribute<ApiVersionAttribute>();
                if (apiVersion?.Versions[0] == selectedApiVersion)
                {
                    yield return apiDescription;
                }
            }
        }


        private IEnumerable<(string key, OpenApiPathItem openApiPathItem)> CollectInfos(OpenApiDocument swaggerDoc, DocumentFilterContext context, ApiVersion selectedApiVersion)
        {
            foreach (var path in swaggerDoc.Paths)
            {
                var apiDescriptions = context.ApiDescriptions.Where(api => api.RelativePath.EqualsTo(path.Key.TrimStart('/'))).ToList();

                foreach (var apiDescription in apiDescriptions)
                {
                    var versionInfo = apiDescription.ActionDescriptor.EndpointMetadata.FirstOrDefaultOfType<ApiVersionAttribute>();

                    // New feature if path without version should be ignored we do not list it any more
                    if (versionInfo.IsNull() && _swaggerInfos.IncludeOnlyVersionedPaths)
                    {
                        continue;
                    }

                    var newOpenApiPathItem = path.Value;
                    if (versionInfo.IsNull())
                    {
                        yield return (path.Key.Replace("{version}", swaggerDoc.Info.Version), newOpenApiPathItem);
                        continue;
                    }

                    if (versionInfo.Versions.Any(v => v.EqualsTo(selectedApiVersion)))
                    {
                        yield return (path.Key.Replace("{version}", swaggerDoc.Info.Version), newOpenApiPathItem);
                    }
                }
            }
        }
    }

    internal sealed record HttpMethodInfo(string HttpMethod, string Route);
}

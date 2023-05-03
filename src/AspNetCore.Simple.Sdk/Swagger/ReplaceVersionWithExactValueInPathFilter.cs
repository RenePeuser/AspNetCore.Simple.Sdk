using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
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

            var collectPathInfos = CollectInfos(swaggerDoc, context, SwaggerUi.SelectedVersion).Distinct(item => item.key).ToList();
            var newPath = new OpenApiPaths();
            collectPathInfos.ForEach(path => newPath.Add(path.key, path.openApiPathItem));
            swaggerDoc.Paths = newPath;
        }


        private IEnumerable<(string key, OpenApiPathItem openApiPathItem)> CollectInfos(OpenApiDocument swaggerDoc, DocumentFilterContext context, ApiVersion selectedApiVersion)
        {
            foreach (var path in swaggerDoc.Paths)
            {
                // Include only version path = true means only path with versions
                if (_swaggerInfos.IncludeOnlyVersionedPaths && path.ToString().DoesNotContain("{version}"))
                {
                    continue;
                }

                // filter all out which are not needed for current version
                var apiDescriptions = context.ApiDescriptions.Where(api => api.RelativePath.EqualsTo(path.Key.TrimStart('/'))).ToList();

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


                    var httpMethods = controller.ControllerTypeInfo.DeclaredMethods.SelectMany(method => method.GetCustomAttributes<HttpMethodAttribute>())
                                                .SelectMany(httpAttribute => httpAttribute.HttpMethods)
                                                .ToImmutableList();

                    var versionInfo = apiDescription.ActionDescriptor.EndpointMetadata.FirstOrDefaultOfType<ApiVersionAttribute>();

                    // New feature if path without version should be ignored we do not list it any more
                    if (versionInfo.IsNull() && _swaggerInfos.IncludeOnlyVersionedPaths)
                    {
                        continue;
                    }

                    var newOpenApiPathItem = path.Value;

                    //// Remove those operations which the controller does not have
                    //// This is a evil part when using versioned swagger documents :/
                    var operationToRemove = newOpenApiPathItem.Operations.Where(operation => httpMethods.Contains(operation.Key.ToString().ToUpperInvariant()).IsFalse()).ToImmutableList();
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
    }
}

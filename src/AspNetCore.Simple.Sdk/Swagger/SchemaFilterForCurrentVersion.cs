using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public partial class SchemaFilterForCurrentVersion : IDocumentFilter
    {
        private static readonly Regex VersionRegex = GetVersionRegex();
        private readonly Assembly _callingAssembly;
        private readonly SwaggerInfos _swaggerInfos;

        public SchemaFilterForCurrentVersion(Assembly callingAssembly, SwaggerInfos swaggerInfos)
        {
            _callingAssembly = callingAssembly;
            _swaggerInfos = swaggerInfos;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // If no paths exits we do not need data types :)
            if (swaggerDoc.Paths.IsEmpty())
            {
                swaggerDoc.Components.Schemas.Clear();
                return;
            }

            // Brand new to know which schemas have to be ignored we have to do following steps:
            // - here we have only the path => namespaces of classes
            // - to unique identify the types we need to know each controllers, routes and types
            // - then we can compare full qualified name to detect if this is to ignore or not.
            var controllers = _callingAssembly.DefinedTypes.Where(type => typeof(ControllerBase).IsAssignableFrom(type)).ToImmutableList();
            var controllerAndTypes = controllers.Select(controller => controller.GetAllTypesForController());
            var assemblyRootName = _callingAssembly.GetName().Name!.ToUpperInvariant();
            var currentVersionSpecificSchema = swaggerDoc.Components.Schemas.Where(keyValue =>
            {
                // Problem to detect type we can not expect that controller and type folders are in the sam sub strcture
                // We have to check if full qualified type names are matching
                var key = keyValue.Key.ToUpperInvariant();

                // If not caller owned namespace we accept all versions
                if (key.Contains(assemblyRootName).IsFalse())
                {
                    return true;
                }

                // We detect the type from swagger to find matching controller to where it will be used
                var matchingType = controllerAndTypes.FirstOrDefault(controllerInfo => controllerInfo.Types.Any(t => t.FullName!.ToUpperInvariant() == key));
                if (matchingType.IsNull())
                {
                    return false;
                }

                // If only version paths are allowed we filter directly out if there is no version
                if (_swaggerInfos.IncludeOnlyVersionedPaths && matchingType.HasVersion.IsFalse())
                {
                    return false;
                }

                // No version return
                if (VersionRegex.Match(key).Success.IsFalse())
                {
                    return true;
                }

                // if path contains current document name == 1.0 (Version)
                if (key.Contains(context.DocumentName.ToUpperInvariant()))
                {
                    return true;
                }

                // normalize version v1
                var normalizedVersion = context.DocumentName.Split('.').FirstOrDefault();
                if (normalizedVersion.IsNullOrWhiteSpace())
                {
                    return false;
                }

                return key.Contains(normalizedVersion.ToUpperInvariant());
            }).ToList();

            // Add needed and version specific schemas
            swaggerDoc.Components.Schemas.ClearAndAddRange(currentVersionSpecificSchema);
        }

        [GeneratedRegex("([V])\\d")]
        private static partial Regex GetVersionRegex();
    }
}

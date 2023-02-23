using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Extensions.Pack;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public partial class SchemaFilterForCurrentVersion : IDocumentFilter
    {
        private static readonly Regex VersionRegex = GetVersionRegex();
        private readonly Assembly _callingAssembly;
        private readonly IImmutableList<string> _pathToIgnore;

        public SchemaFilterForCurrentVersion(Assembly callingAssembly, SwaggerInfos swaggerInfos)
        {
            _callingAssembly = callingAssembly;
            _pathToIgnore = swaggerInfos.PathToIgnore.Split(";",StringSplitOptions.RemoveEmptyEntries).ToImmutableList();
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // If no paths exits we do not need data types :)
            if (swaggerDoc.Paths.IsEmpty())
            {
                swaggerDoc.Components.Schemas.Clear();
                return;
            }

            var assemblyRootName = _callingAssembly.GetName().Name!.ToUpperInvariant();
            var currentVersionSpecificSchema = swaggerDoc.Components.Schemas.Where(keyValue =>
            {
                var key = keyValue.Key.ToUpperInvariant();

                // New feature we can configure paths which we do not want in swagger
                if (_pathToIgnore.Any(path => key.Contains(path.ToUpperInvariant())))
                {
                    return false;
                }

                // If not caller owned namespace we accept all versions
                if (key.Contains(assemblyRootName).IsFalse())
                {
                    return true;
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

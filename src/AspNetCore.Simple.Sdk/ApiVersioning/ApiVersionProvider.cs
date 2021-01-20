using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Extensions.Pack;

namespace AspNetCore.Simple.Sdk.ApiVersioning
{
    internal class ApiVersionProvider
    {
        private readonly AssemblyTypeProvider _assemblyTypeProvider;

        public ApiVersionProvider() : this(new AssemblyTypeProvider())
        {
        }

        public ApiVersionProvider(AssemblyTypeProvider assemblyTypeProvider)
        {
            _assemblyTypeProvider = assemblyTypeProvider;
        }

        internal IEnumerable<ApiVersion> GetAllApiVersions(Assembly assemblies)
        {
            var allApiVersions = _assemblyTypeProvider.GetAllTypes(assemblies)
                                                      .Where(type => type.HasCustomAttribute<ApiVersionAttribute>())
                                                      .SelectMany(type => type.GetCustomAttributes<ApiVersionAttribute>())
                                                      .SelectMany(attribute => attribute.Versions)
                                                      .Distinct()
                                                      .OrderBy(version => version.ToString());

            // If not any controller defines an api version, then we start default with 0.1
            // maybe we find a better way.
            if (allApiVersions.IsEmpty())
            {
                return new ApiVersion(0, 1).ToIList();
            }
            return allApiVersions;
        }
    }
}

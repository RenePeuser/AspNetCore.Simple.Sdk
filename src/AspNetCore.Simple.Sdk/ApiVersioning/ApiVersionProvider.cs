using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApiVersioning
{
    public static class AddApiVersionProviderExtension
    {
        public static void AddApiVersionProvider(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IApiVersionProvider, ApiVersionProvider>();
        }
    }

    public interface IApiVersionProvider
    {
        IImmutableList<ApiVersion> GetAllApiVersions(Assembly assemblies);
    }

    public sealed class ApiVersionProvider(AssemblyTypeProvider assemblyTypeProvider) : IApiVersionProvider
    {
        public ApiVersionProvider() : this(new AssemblyTypeProvider())
        {
        }

        public IImmutableList<ApiVersion> GetAllApiVersions(Assembly assemblies)
        {
            var allApiVersions = assemblyTypeProvider.GetAllTypes(assemblies)
                                                      .Where(type => type.HasCustomAttribute<ApiVersionAttribute>())
                                                      .SelectMany(type => type.GetCustomAttributes<ApiVersionAttribute>())
                                                      .SelectMany(attribute => attribute.Versions)
                                                      .Distinct()
                                                      .OrderBy(version => version.ToString())
                                                      .ToImmutableList();

            return allApiVersions.IsEmpty() ? new ApiVersion(0, 1).ToEnumerable().ToImmutableList() : allApiVersions;
        }
    }
}

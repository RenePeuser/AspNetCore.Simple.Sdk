using System;
using System.Collections.Generic;
using System.Reflection;
using AspNetCore.Simple.Sdk.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApiVersioning
{
    public static class AddAssemblyTypeProviderExtension
    {
        public static void AddAssemblyTypeProvider(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IAssemblyTypeProvider, AssemblyTypeProvider>();
        }
    }

    internal interface IAssemblyTypeProvider
    {
        IEnumerable<Type> GetAllTypes(Assembly assembly);
    }

    internal sealed class AssemblyTypeProvider : IAssemblyTypeProvider
    {
        public IEnumerable<Type> GetAllTypes(Assembly assembly)
        {
            return assembly.GetTypes();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspNetCore.Simple.Sdk.ApiVersioning
{
    internal class AssemblyTypeProvider
    {
        internal IEnumerable<Type> GetAllTypes(Assembly assembly)
        {
            return assembly.GetTypes();
        }
    }
}
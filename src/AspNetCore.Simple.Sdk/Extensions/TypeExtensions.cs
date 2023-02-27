using System;
using System.Collections.Generic;
using Extensions.Pack;

namespace AspNetCore.Simple.Sdk.Extensions;

internal static class TypeExtensions
{
    internal static IEnumerable<Type> GetAllGenericArguments(this Type type)
    {
        var genericArguments = type.GetGenericArguments();
        foreach (var genericArgument in genericArguments)
        {
            var subTypes = GetAllGenericArguments((genericArgument));
            foreach (var subType in subTypes)
            {
                yield return subType;
            }

            yield return genericArgument;
        }
    }

    internal static IEnumerable<Type> GetAllTypesFromGenericType(this Type type)
    {
        if (type.IsGenericType.IsFalse())
        {
            yield return type;
        }

        var genericArguments = type.GetGenericArguments();
        foreach (var genericArgument in genericArguments)
        {
            yield return genericArgument;
        }
    }
}
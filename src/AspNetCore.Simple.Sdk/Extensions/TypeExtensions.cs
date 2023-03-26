using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Simple.Sdk.Extensions
{
    public static class TypeExtensions
    {
        internal static IEnumerable<Type> GetAllGenericArguments(this Type type)
        {
            var genericArguments = type.GetGenericArguments();
            foreach (var genericArgument in genericArguments)
            {
                var subTypes = GetAllGenericArguments(genericArgument);
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

        public static ControllerTypesInfo GetAllTypesForController(this TypeInfo type)
        {
            if (typeof(ControllerBase).IsAssignableFrom(type).IsFalse())
            {
                throw new ProblemDetailsException($"Only types derived from {typeof(ControllerBase)} are allowed to use here");
            }

            var controller = type;

            var methods = controller.DeclaredMethods.ToImmutableList();
            var types = methods.SelectMany(method => method.GetParameters().Concat(method.ReturnParameter)).Select(parameter => parameter.ParameterType).ToImmutableList();
            var allGenericTypes = types.SelectMany(t => t.GetAllTypesFromGenericType()).ToImmutableList();
            var allTypes = GetAllSubTypes(types.Concat(allGenericTypes).ToImmutableList(), new List<string>()).ToImmutableList();
            var hasVersion = HasVersion(controller);

            return new ControllerTypesInfo(controller, allTypes, hasVersion);
        }

        public static ControllerTypesInfoLegacy GetAllTypesForController(this Type type)
        {
            if (typeof(ControllerBase).IsAssignableFrom(type).IsFalse())
            {
                throw new ProblemDetailsException($"Only types derived from {typeof(ControllerBase)} are allowed to use here");
            }

            var controller = type;

            var methods = controller.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).ToImmutableList();
            var types = methods.SelectMany(method => method.GetParameters().Concat(method.ReturnParameter)).Select(parameter => parameter.ParameterType).ToImmutableList();
            var allGenericTypes = types.SelectMany(t => t.GetAllTypesFromGenericType()).ToImmutableList();
            var allTypes = GetAllSubTypes(types.Concat(allGenericTypes).ToImmutableList(), new List<string>()).ToImmutableList();
            var hasVersion = HasVersion(controller);

            return new ControllerTypesInfoLegacy(controller, allTypes, hasVersion);
        }

        public record ControllerTypesInfo(TypeInfo Controller,
                                          IImmutableList<Type> Types,
                                          bool HasVersion);
        public record ControllerTypesInfoLegacy(Type Controller,
                                                IImmutableList<Type> Types,
                                                bool HasVersion);


        private static bool HasVersion(Type typeInfo)
        {
            //  [ApiVersionNeutral]
            if (typeInfo.HasCustomAttribute<ApiVersionNeutralAttribute>())
            {
                return false;
            }

            return typeInfo.HasCustomAttribute<ApiVersionAttribute>();
        }

        private static bool HasVersion(TypeInfo typeInfo)
        {
            //  [ApiVersionNeutral]
            if (typeInfo.HasCustomAttribute<ApiVersionNeutralAttribute>())
            {
                return false;
            }

            return typeInfo.HasCustomAttribute<ApiVersionAttribute>();
        }

        private static IEnumerable<Type> GetAllSubTypes(IImmutableList<Type> types, List<string> alreadyFound)
        {
            foreach (var type in types)
            {
                if (alreadyFound.Contains(type.FullName!))
                {
                    continue;
                }

                if (type.FullName.IsNull())
                {
                    continue;
                }

                // we are not interested in system types or any type from microsoft
                if (type.IsSystemType())
                {
                    continue;
                }

                if (type.FullName.Contains("Microsoft."))
                {
                    continue;
                }

                var properties = type.GetProperties().Select(p => p.PropertyType).ToImmutableList();
                var genericArguments = properties.SelectMany(p => p.GetGenericArguments());
                var allPropertyTypes = properties.Concat(genericArguments).Where(p => p.FullName.NotEqualsTo(type.FullName)).ToImmutableList(); // to avoid recursion to infinity

                var subTypes = GetAllSubTypes(allPropertyTypes, alreadyFound).ToImmutableList();
                foreach (var subType in subTypes)
                {
                    alreadyFound.Add(subType.FullName!);
                    yield return subType;
                }

                alreadyFound.Add(type.FullName!);
                yield return type;
            }
        }
    }
}

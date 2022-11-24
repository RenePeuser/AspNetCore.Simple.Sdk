using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Extensions.Pack;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal sealed class ImplementationFinderForInterface
    {
        internal IImmutableList<Type> FindFor(Type interfaceType)
        {
            if (interfaceType.IsInterface.IsFalse())
            {
                // Use new argument check :)
            }


            var implementations = interfaceType.Assembly.GetTypes()
                                                        .Where(type => type.IsInterface.IsFalse() && interfaceType.IsAssignableFrom(type))
                                                        .ToImmutableList();

            return implementations;
        }
    }

    internal sealed class DependencyDetector
    {
        private readonly ImplementationFinderForInterface _implementationFinderForInterface;

        public DependencyDetector(ImplementationFinderForInterface implementationFinderForInterface)
        {
            _implementationFinderForInterface = implementationFinderForInterface;
        }

        internal IImmutableList<Type> FindDependenciesFor(Type type)
        {
            var dependencies = FindDependenciesForInternal(type);
            var filterDuplicate = dependencies.Distinct(dependency => dependency.FullName!);
            return filterDuplicate.ToImmutableList();
        }


        private IImmutableList<Type> FindDependenciesForInterface(Type interfaceType)
        {
            var implementations = _implementationFinderForInterface.FindFor(interfaceType);
            var allDependencies = implementations.SelectMany(FindDependenciesForInternal).ToImmutableList();
            return allDependencies;
        }

        private IEnumerable<Type> FindDependenciesForInternal(Type type)
        {
            // First we have to detect the constructor with the most parameters !!
            // Hint: Maybe add an attribute like MEF or JSON which should be the correct injection constructor
            //       In good designs with correct constructor implementations there should be only one with the correct dependencies
            var parameterInfos = type.GetConstructors()
                                     .Select(ctor => new { Parameters = ctor.GetParameters(), Constructor = ctor })
                                     .MaxBy(ctorInfo => ctorInfo.Parameters.Length)?.Parameters;

            if (parameterInfos is null)
            {
                yield break;
            }

            foreach (var parameterInfo in parameterInfos)
            {
                if (typeof(IEnumerable).IsAssignableFrom(parameterInfo.ParameterType))
                {
                    if (parameterInfo.ParameterType.IsGenericType.IsFalse())
                    {
                        throw new InvalidOperationException(
                            $"It is not possible to inject a non generic list type. Your type: '{parameterInfo.ParameterType}' is not defining a generic type. It is not possible to inject any dependencies of unknown type.");
                    }

                    var generiyType = parameterInfo.ParameterType.GetGenericArguments();
                    if (generiyType.Length > 1)
                    {
                        throw new InvalidOperationException($"It is not possible to inject a multi generic type list. Your type: '{parameterInfo.ParameterType}'. Your type definition is to complex to find a correct implementation for.");
                    }

                    var dependencies = FindDependenciesFor(generiyType.First());
                    foreach (var dependency in dependencies)
                    {
                        yield return dependency;
                    }
                }


                if (parameterInfo.ParameterType.IsInterface)
                {
                    var depdenciesByInterfaces = FindDependenciesForInterface(parameterInfo.ParameterType);
                    foreach (var dependencyByInterface in depdenciesByInterfaces)
                    {
                        yield return dependencyByInterface;
                    }
                }
                else
                {
                    var nextDependencies = FindDependenciesForInternal(parameterInfo.ParameterType);
                    foreach (var nextDependency in nextDependencies)
                    {
                        yield return nextDependency;
                    }
                }

                yield return parameterInfo.ParameterType;
            }

            yield return type;
        }
    }
}

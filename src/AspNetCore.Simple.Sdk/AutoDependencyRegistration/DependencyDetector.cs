using System;
using System.Collections.Generic;
using System.Linq;
using Extensions.Pack;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal class DependencyDetector
    {
        internal IEnumerable<Type> FindDependenciesFor(Type type)
        {
            var dependencies = FindDependenciesForInternal(type);
            var filterDuplicate = dependencies.Distinct(dependency => dependency.FullName);
            return filterDuplicate;
        }

        private IEnumerable<Type> FindDependenciesForInternal(Type type)
        {
            // First we have to detect the constructor with the most parameters !!
            // Hint: Maybe add an attribute like MEF or JSON which should be the correct injection constructor
            //       In good designs with correct constructor implementations there should be only one with the correct dependencies
            var parameterInfos = type.GetConstructors()
                                     .Select(ctor => new { Parameters = ctor.GetParameters(), Constructor = ctor })
                                     .OrderByDescending(ctorInfo => ctorInfo.Parameters.Length)
                                     .FirstOrDefault()?.Parameters;

            if (parameterInfos.IsNullOrEmpty())
            {
                yield break;
            }

            foreach (var parameterInfo in parameterInfos)
            {
                var nextDependencies = FindDependenciesForInternal(parameterInfo.ParameterType);
                foreach (var nextDependency in nextDependencies)
                {
                    yield return nextDependency;
                }

                yield return parameterInfo.ParameterType;
            }
        }
    }
}
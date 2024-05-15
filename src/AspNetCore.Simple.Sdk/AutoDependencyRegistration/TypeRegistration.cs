using System;
using System.Collections.Generic;
using System.Linq;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal sealed class TypeRegistration
    {
        private readonly IEnumerable<IRegistrationStrategy> _registrationStrategies;
        private readonly RegistrationCheck _registrationCheck;
        private readonly DependencyDetector _dependencyDetector;

        public TypeRegistration(IEnumerable<IRegistrationStrategy> registrationStrategies,
                                RegistrationCheck registrationCheck,
                                DependencyDetector dependencyDetector)
        {
            _registrationStrategies = registrationStrategies;
            _registrationCheck = registrationCheck;
            _dependencyDetector = dependencyDetector;
        }

        internal void DoAutoRegistration(Type type)
        {
            // 1. Check if this is already registered
            if (_registrationCheck.IsAlreadyRegistered(type))
            {
                return;
            }

            // 2. Detect dependencies 
            var dependencies = _dependencyDetector.FindDependenciesFor(type).ToList();

            // 3. Register dependencies first
            foreach (var dependency in dependencies)
            {
                DoAutoRegistrationInternal(dependency);
            }

            // 4. Register root type
            DoAutoRegistrationInternal(type);
        }

        private void DoAutoRegistrationInternal(Type type)
        {
            if (_registrationCheck.IsAlreadyRegistered(type))
            {
                return;
            }

            // if you use your controller as entry root registration point, all dependencies was already registered step before :)
            if (typeof(ControllerBase).IsAssignableFrom(type))
            {
                return;
            }

            var registrationResult = _registrationStrategies.Aggregate(false, (current, registrationStrategy) => registrationStrategy.DoRegistrationFor(type, current));
            if (registrationResult.IsFalse())
            {
                throw new MissingRegistrationStrategyException(
                    $"For type: '{type.Name}' in namespace: '{type.Namespace}' we do not have a strategy to register it correctly. Please check that you use: '{nameof(ServiceRegistrationAttribute)}' for services or '{nameof(AppSettingsRegistrationAttribute)}' for any kind of app settings, if your declaration is not obvious.");
            }
        }
    }
}

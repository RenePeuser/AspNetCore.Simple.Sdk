using System;
using System.Linq;
using System.Reflection;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal sealed class ServiceRegistration : IRegistrationStrategy
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly LifetimeDetector _lifetimeDetector;
        private readonly InterfaceDetector _interfaceDetector;
        private readonly BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public ServiceRegistration(IServiceCollection serviceCollection,
                                   LifetimeDetector lifetimeDetector,
                                   InterfaceDetector interfaceDetector)
        {
            _serviceCollection = serviceCollection;
            _lifetimeDetector = lifetimeDetector;
            _interfaceDetector = interfaceDetector;
        }

        public bool DoRegistrationFor(Type type, bool registrationDone)
        {
            if (registrationDone)
            {
                return true;
            }

            // If we have an app settings registration it is not our responsibility to register it.
            if (type.HasCustomAttribute<AppSettingsRegistrationAttribute>())
            {
                return false;
            }

            // A service must have declared methods otherwise it is a data class !
            var properties = type.GetProperties(_bindingFlags);
            if (properties.Any())
            {
                return false;
            }

            //// if we do not have properties and no methods this is an unknown state without explicit type registration not to know hot to register it.
            //if (type.GetMethods(_bindingFlags).IsEmpty())
            //{
            //    return false;
            //}

            var interfaceToRegisterFor = _interfaceDetector.DetectInterface(type);
            var lifetime = _lifetimeDetector.DetectFor(type);
            var interfaceType = interfaceToRegisterFor ?? type;
            // If no interface exists we register the same type for interface and implementation !
            _serviceCollection.Add(new ServiceDescriptor(interfaceType, type, lifetime));
            return true;
        }
    }
}

using System;
using System.Linq;
using System.Reflection;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal sealed class ServiceRegistration(IServiceCollection serviceCollection,
                                              LifetimeDetector lifetimeDetector,
                                              InterfaceDetector interfaceDetector) : IRegistrationStrategy
    {
        private readonly BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

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

            var interfaceToRegisterFor = interfaceDetector.DetectInterface(type);
            var lifetime = lifetimeDetector.DetectFor(type);
            var interfaceType = interfaceToRegisterFor ?? type;
            // If no interface exists we register the same type for interface and implementation !
            serviceCollection.Add(new ServiceDescriptor(interfaceType, type, lifetime));
            return true;
        }
    }
}

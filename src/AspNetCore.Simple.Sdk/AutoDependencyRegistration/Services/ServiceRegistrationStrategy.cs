using System;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal class ServiceRegistrationStrategy : IRegistrationStrategy
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly LifetimeDetector _lifetimeDetector;
        private readonly InterfaceDetector _interfaceDetector;

        public ServiceRegistrationStrategy(IServiceCollection serviceCollection, LifetimeDetector lifetimeDetector, InterfaceDetector interfaceDetector)
        {
            _serviceCollection = serviceCollection;
            _lifetimeDetector = lifetimeDetector;
            _interfaceDetector = interfaceDetector;
        }

        public bool DoAutoRegistration(Type type, bool registrationDone)
        {
            if (registrationDone)
            {
                return true;
            }

            if (type.HasCustomAttribute<AppSettingsRegistrationAttribute>())
            {
                return false;
            }

            var interfaceToRegisterFor = _interfaceDetector.DetectInterface(type);
            var lifetime = _lifetimeDetector.DetectFor(type);
            var interfaceType = interfaceToRegisterFor ?? type;
            // If no interface exists we register the same type for interface and implementation !
            _serviceCollection.Add(new ServiceDescriptor(interfaceType, type, lifetime));
            return true;
        }
    }
}
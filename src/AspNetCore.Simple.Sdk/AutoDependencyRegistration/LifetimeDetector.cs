using System;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    public class LifetimeDetector
    {
        public ServiceLifetime DetectFor<T>()
        {
            return DetectFor(typeof(T));
        }

        public ServiceLifetime DetectFor(Type type)
        {
            var lifetimeAttribute = type.GetCustomAttribute<ServiceRegistrationAttribute>();
            if (lifetimeAttribute.IsNull())
            {
                return ServiceLifetime.Singleton;
            }

            return lifetimeAttribute.ServiceLifetime;
        }
    }
}

using System;
using System.Linq;
using Extensions.Pack;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal class InterfaceDetector
    {
        internal Type? DetectInterface(Type typeToRegister)
        {
            // 1. Explicit given interface type has priority 1.
            var serviceRegistration = typeToRegister.GetCustomAttribute<ServiceRegistrationAttribute>();
            if (serviceRegistration?.InterfaceType is not null)
            {
                return serviceRegistration.InterfaceType;
            }

            // 2. Check if no explicit interface type was given, the amount of interfaces
            var interfaces = typeToRegister.GetInterfaces();

            // 3. If we have exactly only one interface we use that one.
            //    Important if this auto detected interface is not what you want, please provide the explicit interface, or null for the interface type registration.
            if (interfaces.Length == 1)
            {
                return interfaces.First();
            }

            return null;
        }
    }
}
using System.Collections.Generic;
using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.WithInterface.Services
{
    [ServiceRegistration(ServiceLifetime.Scoped)]
    public class ScopedServiceWithInterface : IScopedService
    {
        public bool DoSomething()
        {
            return true;
        }
    }

    public interface IScopedService
    {
        bool DoSomething();
    }

    [ServiceRegistration(ServiceLifetime.Scoped)]
    public class ScopedServiceWithInterfaceDependencies(IScopedService scopedService)
    {
        public bool DoSomething()
        {
            return scopedService.DoSomething();
        }
    }


    [ServiceRegistration(ServiceLifetime.Scoped)]
    public class Strategy1 : IStrategy
    {
    }

    [ServiceRegistration(ServiceLifetime.Scoped)]
    public class Strategy2 : IStrategy
    {
    }

    public interface IStrategy
    {
        bool DoSomething()
        {
            return true;
        }
    }

    [ServiceRegistration(ServiceLifetime.Scoped)]
    public class ScopedServiceWithListOfInterfaceDependencies(IEnumerable<IStrategy> strategies)
    {
        public bool DoSomething()
        {
            var result = false;
            foreach (var strategy in strategies)
            {
                result = strategy.DoSomething();
            }

            return result;
        }
    }
}

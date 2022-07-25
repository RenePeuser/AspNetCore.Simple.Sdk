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
    public class ScopedServiceWithInterfaceDependencies
    {
        private readonly IScopedService _scopedService;

        public ScopedServiceWithInterfaceDependencies(IScopedService scopedService)
        {
            _scopedService = scopedService;
        }


        public bool DoSomething()
        {
            return _scopedService.DoSomething();
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
    public class ScopedServiceWithListOfInterfaceDependencies
    {
        private readonly IEnumerable<IStrategy> _strategies;

        public ScopedServiceWithListOfInterfaceDependencies(IEnumerable<IStrategy> strategies)
        {
            _strategies = strategies;
        }


        public bool DoSomething()
        {
            var result = false;
            foreach (var strategy in _strategies)
            {
                result = strategy.DoSomething();
            }

            return result;
        }
    }
}

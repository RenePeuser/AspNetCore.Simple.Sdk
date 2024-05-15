using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.ServiceHierarchy.Services
{
    [ServiceRegistration(ServiceLifetime.Scoped)]
    public class ScopedRootRootService : IScopedRootService
    {
        private readonly ServiceHierarchy01 _serviceHierarchy01;

        public ScopedRootRootService(ServiceHierarchy01 serviceHierarchy01)
        {
            _serviceHierarchy01 = serviceHierarchy01;
        }

        public void DoSomething()
        {
            _serviceHierarchy01.DoSomething();
        }
    }

    public interface IScopedRootService
    {
    }

    public class ServiceHierarchy01
    {
        private readonly ServiceHierarchy02 _serviceHierarchy02;

        public ServiceHierarchy01(ServiceHierarchy02 serviceHierarchy02)
        {
            _serviceHierarchy02 = serviceHierarchy02;
        }

        public void DoSomething()
        {
            _serviceHierarchy02.DoSomething();
        }
    }

    public class ServiceHierarchy02
    {
        public void DoSomething()
        {
        }
    }
}

using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.ServiceHierarchy.Services
{
    [ServiceRegistration(ServiceLifetime.Scoped)]
    public class ScopedRootRootService(ServiceHierarchy01 serviceHierarchy01) : IScopedRootService
    {
        public void DoSomething()
        {
            serviceHierarchy01.DoSomething();
        }
    }

    public interface IScopedRootService
    {
    }

    public class ServiceHierarchy01(ServiceHierarchy02 serviceHierarchy02)
    {
        public void DoSomething()
        {
            serviceHierarchy02.DoSomething();
        }
    }

    public class ServiceHierarchy02
    {
        public void DoSomething()
        {
        }
    }
}

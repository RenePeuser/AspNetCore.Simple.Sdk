using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.ServiceHierarchy.Services
{
    [ServiceRegistration(ServiceLifetime.Scoped)]
    internal sealed class InterfaceScopedRootRootService(IServiceHierarchy01 serviceHierarchy01) : IInterfaceScopedRootService
    {
        public void DoSomething()
        {
            serviceHierarchy01.DoSomething();
        }
    }

    public interface IInterfaceScopedRootService
    {
        void DoSomething();
    }

    internal sealed class InterfaceServiceHierarchy01(IServiceHierarchy02 serviceHierarchy02) : IServiceHierarchy01
    {
        public void DoSomething()
        {
            serviceHierarchy02.DoSomething();
        }
    }

    public interface IServiceHierarchy01
    {
        void DoSomething();
    }

    internal sealed class InterfaceServiceHierarchy02 : IServiceHierarchy02
    {
        public void DoSomething()
        {
        }
    }

    public interface IServiceHierarchy02
    {
        void DoSomething();
    }
}

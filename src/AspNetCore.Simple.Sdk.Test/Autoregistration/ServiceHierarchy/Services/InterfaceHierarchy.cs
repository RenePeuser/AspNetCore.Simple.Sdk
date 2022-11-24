using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.ServiceHierarchy.Services
{
    [ServiceRegistration(ServiceLifetime.Scoped)]
    internal sealed class InterfaceScopedRootRootService : IInterfaceScopedRootService
    {
        private readonly IServiceHierarchy01 _serviceHierarchy01;

        public InterfaceScopedRootRootService(IServiceHierarchy01 serviceHierarchy01)
        {
            _serviceHierarchy01 = serviceHierarchy01;
        }

        public void DoSomething()
        {
            _serviceHierarchy01.DoSomething();
        }
    }

    public interface IInterfaceScopedRootService
    {
        void DoSomething();
    }

    internal sealed class InterfaceServiceHierarchy01 : IServiceHierarchy01
    {
        private readonly IServiceHierarchy02 _serviceHierarchy02;

        public InterfaceServiceHierarchy01(IServiceHierarchy02 serviceHierarchy02)
        {
            _serviceHierarchy02 = serviceHierarchy02;
        }

        public void DoSomething()
        {
            _serviceHierarchy02.DoSomething();
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

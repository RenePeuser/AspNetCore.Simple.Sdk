using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.ServiceHierarchy.Services
{
    [ServiceRegistration(ServiceLifetime.Scoped)]
    public class ScopedRootRootService : IScopedRootService
    {
        public ScopedRootRootService(ServiceHierarchy01 serviceHierarchy01)
        {
            ServiceHierarchy01 = serviceHierarchy01;
        }

        public ServiceHierarchy01 ServiceHierarchy01 { get; }
    }

    public interface IScopedRootService
    {
    }

    public class ServiceHierarchy01
    {
        public ServiceHierarchy01(ServiceHierarchy02 serviceHierarchy02)
        {
            ServiceHierarchy02 = serviceHierarchy02;
        }

        public ServiceHierarchy02 ServiceHierarchy02 { get; }
    }

    public class ServiceHierarchy02
    {

    }
}

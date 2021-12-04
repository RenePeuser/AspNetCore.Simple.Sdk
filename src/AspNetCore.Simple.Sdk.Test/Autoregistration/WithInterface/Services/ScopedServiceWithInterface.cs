using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.WithInterface.Services
{
    [ServiceRegistration(ServiceLifetime.Scoped)]
    public class ScopedServiceWithInterface : IScopedService
    {
    }

    public interface IScopedService
    {
    }
}

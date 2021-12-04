using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.WithInterface.Services
{
    [ServiceRegistration(ServiceLifetime.Singleton)]
    public class SingletonServiceWithInterface : ISingletonService
    {
    }

    public interface ISingletonService
    {
    }
}

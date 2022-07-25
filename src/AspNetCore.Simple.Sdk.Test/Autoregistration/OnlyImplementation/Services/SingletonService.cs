using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.OnlyImplementation.Services
{
    [ServiceRegistration(ServiceLifetime.Singleton)]
    public class SingletonService
    {
    }
}

using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.WithInterface.Services
{
    [ServiceRegistration(ServiceLifetime.Transient)]
    public class TransientServiceWithInterface : ITransientService
    {
    }

    public interface ITransientService
    {
    }
}

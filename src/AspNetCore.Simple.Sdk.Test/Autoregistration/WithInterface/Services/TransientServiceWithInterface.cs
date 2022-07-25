using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.WithInterface.Services
{
    [ServiceRegistration(ServiceLifetime.Transient)]
    public class TransientServiceWithInterface : ITransientService
    {
    }

    [ServiceRegistration(ServiceLifetime.Transient, typeof(ITransientServiceSpecific))]
    public class TransientServiceWithSpecificInterface : ITransientService, ITransientServiceSpecific
    {
    }

    public interface ITransientService
    {
    }

    public interface ITransientServiceSpecific
    {
    }
}

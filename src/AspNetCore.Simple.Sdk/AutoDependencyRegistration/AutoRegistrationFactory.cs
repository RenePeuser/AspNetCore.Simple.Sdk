using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    public class AutoRegistrationFactory
    {
        public AutoRegistration Create(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var lifetimeDetector = new LifetimeDetector();
            var interfaceDetector = new InterfaceDetector();

            var appsettingsRegistrationStrategy = new AppSettingsRegistration(lifetimeDetector, serviceCollection, configuration);
            var simpleAppsettingsRegistration = new SimpleAppSettingsRegistration(lifetimeDetector, serviceCollection, configuration);
            var serviceRegistrationStrategy = new ServiceRegistration(serviceCollection, lifetimeDetector, interfaceDetector);
            var customRegistrationStrategy = new CustomRegistration(serviceCollection, configuration);

            var registrationCheck = new RegistrationCheck(serviceCollection);
            var implementationFinderForInterface = new ImplementationFinderForInterface();
            var dependencyDetector = new DependencyDetector(implementationFinderForInterface);
            var registrationStrategy = new TypeRegistration(new IRegistrationStrategy[] { customRegistrationStrategy, simpleAppsettingsRegistration, appsettingsRegistrationStrategy, serviceRegistrationStrategy }, registrationCheck,
                dependencyDetector);
            var autoRegistration = new AutoRegistration(registrationStrategy);
            return autoRegistration;
        }
    }
}

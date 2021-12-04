using System;
using System.IO;
using System.Linq;
using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using AspNetCore.Simple.Sdk.Test.Autoregistration.AppSettings.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.AppSettings
{
    [TestClass]
    public class App_Settings_Registrations
    {
        private AutoRegistration _autoRegistration;
        private ServiceCollection _serviceCollection;

        [TestInitialize]
        public void Init()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var testAppsettingsJson = Path.Combine(Environment.CurrentDirectory, "Autoregistration", "AppSettings", "Json", "appsettings.test.json");
            configurationBuilder.AddJsonFile(testAppsettingsJson);

            _serviceCollection = new ServiceCollection();
            var configuration = configurationBuilder.Build();

            _autoRegistration = new AutoRegistrationFactory().Create(_serviceCollection, configuration);
        }

        [DataTestMethod]
        [DataRow(typeof(ScopedSettings))]
        [DataRow(typeof(TransientSettings))]
        [DataRow(typeof(SingletonSettings))]
        public void Should_Register_AppSettings_Without_Any_Validation_Issues(Type type)
        {
            _autoRegistration.DoAutoRegistrationFor(type);

            var serviceProvider = _serviceCollection.BuildServiceProvider();
            var instance = serviceProvider.GetService(type);

            Assert.IsNotNull(instance);
        }

        [DataTestMethod]
        [DataRow(typeof(ScopedSettingsWithValidator))]
        public void Should_Not_Be_Able_To_Register_AppSettings_Because_Of_Validation_Errors(Type type)
        {
            Assert.ThrowsException<ArgumentException>(() => _autoRegistration.DoAutoRegistrationFor(type));
        }

        [TestMethod]
        public void Should_Not_Be_Able_To_Register_AppSettings_Because_Of_Validation_Errors()
        {
            Assert.ThrowsException<ArgumentException>(() => _autoRegistration.DoAutoRegistrationFor<ScopedSettingsWithValidator>());
        }
    }

    [TestClass]
    public class App_Settings_Validation_Test
    {
        private AutoRegistration _autoRegistration;
        private ServiceCollection _serviceCollection;

        [TestInitialize]
        public void Init()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var testAppsettingsJson = Path.Combine(Environment.CurrentDirectory, "Autoregistration", "AppSettings", "Json", "appsettings.test.json");
            configurationBuilder.AddJsonFile(testAppsettingsJson);

            _serviceCollection = new ServiceCollection();
            var configuration = configurationBuilder.Build();

            _autoRegistration = new AutoRegistrationFactory().Create(_serviceCollection, configuration);
        }

        [DataTestMethod]
        [DataRow(typeof(ScopedSettings), ServiceLifetime.Scoped)]
        [DataRow(typeof(TransientSettings), ServiceLifetime.Transient)]
        [DataRow(typeof(SingletonSettings), ServiceLifetime.Singleton)]
        public void Should_Register_AppSettings_With_Expected_Lifetime(Type type, ServiceLifetime serviceLifetime)
        {
            _autoRegistration.DoAutoRegistrationFor(type);

            var serviceProvider = _serviceCollection.BuildServiceProvider();
            var instance = serviceProvider.GetService(type);

            Assert.IsNotNull(instance);
            Assert.AreEqual(1, _serviceCollection.Count);

            var serviceRegistration = _serviceCollection.First();

            Assert.AreEqual(type, serviceRegistration.ServiceType);
            Assert.IsNull(serviceRegistration.ImplementationType);
            Assert.AreEqual(serviceLifetime, serviceRegistration.Lifetime);
        }
    }
}

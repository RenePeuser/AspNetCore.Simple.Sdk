using System;
using System.IO;
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
}

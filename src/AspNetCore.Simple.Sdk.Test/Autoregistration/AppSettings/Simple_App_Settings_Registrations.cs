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
    public class Simple_App_Settings_Registrations
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

        [TestMethod]
        public void Should_Register_Automatically_Simple_Settings_Class_By_Generic_Type()
        {
            _autoRegistration.DoAutoRegistrationFor<SimpleSettings>();

            var serviceProvider = _serviceCollection.BuildServiceProvider();
            var instance = serviceProvider.GetService<SimpleSettings>();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void Should_Register_Automatically_Simple_Settings_Class_By_Type_Info()
        {
            var type = typeof(SimpleSettings);

            _autoRegistration.DoAutoRegistrationFor(type);

            var serviceProvider = _serviceCollection.BuildServiceProvider();

            var instance = serviceProvider.GetService(type);

            Assert.IsNotNull(instance);
        }
    }
}

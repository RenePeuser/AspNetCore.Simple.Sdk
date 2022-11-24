using System;
using System.IO;
using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.CustomRegistration
{

    public class CustomRegistrationForMyService : ICustomTypeRegistration
    {
        public void Register(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingletonIfNotExists<ServiceByCustomRegistration>();
        }
    }

    [CustomRegistration(typeof(CustomRegistrationForMyService))]
    public class ServiceByCustomRegistration
    {
        public bool Invoke() => true;
    }

    public class CustomRegistrationForSettings : ICustomTypeRegistration
    {
        public void Register(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var settings = configuration.GetSetting<SettingsByCustomRegistration>("Settings");

            if (settings.IsNull())
            {
                throw new NotImplementedException();
            }

            serviceCollection.AddSingleton(settings);
        }
    }

    [CustomRegistration(typeof(CustomRegistrationForSettings))]
    public class SettingsByCustomRegistration
    {
        public string Name { get; init; } = string.Empty;
    }


    [TestClass]
    public class Test_Custom_Registration
    {
        private IConfigurationRoot _configuration = null!;
        private ServiceCollection _serviceCollection = null!;
        private AutoRegistration _autoRegistration = null!;

        [TestInitialize]
        public void Init()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var testAppsettingsJson = Path.Combine(Environment.CurrentDirectory, "Autoregistration", "AppSettings", "Json", "appsettings.test.json");
            configurationBuilder.AddJsonFile(testAppsettingsJson);


            _configuration = configurationBuilder.Build();
            _serviceCollection = new ServiceCollection();
            _autoRegistration = new AutoRegistrationFactory().Create(_serviceCollection, _configuration);
        }

        [TestMethod]
        public void Should_Be_Able_To_Use_Custom_Registration_For_Service()
        {
            _autoRegistration.DoAutoRegistrationFor<ServiceByCustomRegistration>();

            var serviceProvider = _serviceCollection.BuildServiceProvider();

            var result = serviceProvider.GetService<ServiceByCustomRegistration>();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Invoke());

            var result2 = serviceProvider.GetService<ServiceByCustomRegistration>();

            Assert.AreEqual(result, result2);
        }

        [TestMethod]
        public void Should_Be_Able_To_Use_Custom_Registration_For_AppSettings()
        {
            _autoRegistration.DoAutoRegistrationFor<SettingsByCustomRegistration>();

            var serviceProvider = _serviceCollection.BuildServiceProvider();

            var result = serviceProvider.GetService<SettingsByCustomRegistration>();

            Assert.IsNotNull(result);
            Assert.AreEqual("SonGoku", result.Name);
        }
    }
}

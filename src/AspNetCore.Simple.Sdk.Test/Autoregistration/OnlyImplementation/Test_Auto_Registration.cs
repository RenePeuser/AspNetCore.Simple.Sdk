using System;
using System.CodeDom;
using System.Runtime.InteropServices.ComTypes;
using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using AspNetCore.Simple.Sdk.Test.Autoregistration.OnlyImplementation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.OnlyImplementation
{
    [TestClass]
    public class Test_Auto_Registration
    {
        private IConfigurationRoot _configuration;
        private ServiceCollection _serviceCollection;
        private AutoRegistration _autoregister;

        [TestInitialize]
        public void Init()
        {
            _configuration = new ConfigurationBuilder().Build();
            _serviceCollection = new ServiceCollection();
            _autoregister = new AutoRegistrationFactory().Create(_serviceCollection, _configuration);
        }

        [TestMethod]
        public void Should_Regsister_Simple_Service_Without_Dependency_With_Default_As_Singleton()
        {
            _autoregister.DoAutoRegistrationFor<SimpleService>();

            var serviceProvider = _serviceCollection.BuildServiceProvider();

            var result = serviceProvider.GetService<SimpleService>();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Invoke());

            var result2 = serviceProvider.GetService<SimpleService>();

            Assert.AreEqual(result, result2);
        }

        [DataTestMethod]
        [DataRow(typeof(TransientService))]
        public void Should_Register_Simple_Scoped_Service_Without_Dependency_With_Default_As_Singleton(Type service)
        {
            _autoregister.DoAutoRegistrationFor(service);
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            var result = serviceProvider.GetService(service);
            var result2 = serviceProvider.GetService(service);

            Assert.AreNotEqual(result, result2);
        }

        [TestMethod]
        public void Should_Register_Simple_Transient_Service_Without_Dependency_With_Default_As_Singleton()
        {
            _autoregister.DoAutoRegistrationFor<TransientService>();
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            var result = serviceProvider.GetService<TransientService>();
            var result2 = serviceProvider.GetService<TransientService>();

            Assert.IsNotNull(result);
            Assert.AreNotEqual(result, result2);
        }

        [TestMethod]
        public void Should_Register_Simple_Singleton_Service_Without_Dependency_With_Default_As_Singleton()
        {
            _autoregister.DoAutoRegistrationFor<SingletonService>();
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            var result = serviceProvider.GetService<SingletonService>();
            var result2 = serviceProvider.GetService<SingletonService>();

            Assert.IsNotNull(result);
            Assert.AreEqual(result, result2);
        }
    }
}

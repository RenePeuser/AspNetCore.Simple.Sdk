using System;
using System.IO;
using AspNetCore.Simple.MsTest.Sdk;
using Extensions.Pack;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test.ConfigurationExtensions
{
    public abstract class ConfigurationTestBase
    {
        protected IConfigurationRoot Configuration { get; private set; } = null!;

        [TestInitialize]
        public void Setup()
        {
            // 1. Fetching secrets to get a connection string, for a test database
            var fileStream = this.GetType().Assembly.GetEmbeddedFileStream("ConfigurationExtensions.appsettings.test.json");

            Configuration = new ConfigurationBuilder().AddJsonStream(fileStream.Stream).Build();
        }
    }
}

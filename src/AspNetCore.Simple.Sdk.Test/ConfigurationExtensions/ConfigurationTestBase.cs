using System.IO;
using Extensions.Pack;
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
            var fileStream = GetType().Assembly.GetEmbeddedFileStream("ConfigurationExtensions.appsettings.test.json");
            Configuration = new ConfigurationBuilder().AddJsonStream(fileStream.Stream).Build();
        }
    }
}

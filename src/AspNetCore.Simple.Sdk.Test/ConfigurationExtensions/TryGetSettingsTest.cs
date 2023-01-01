using AspNetCore.Simple.MsTest.Sdk;
using Extensions.Pack;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test.ConfigurationExtensions
{
    [TestCategory("Configuration")]
    public class TryGetSettingsTest : ConfigurationTestBase
    {
        [TestMethod]
        public void Should_Be_Able_To_Fetch_Settings_Directly_By_Typename_Without_Settings_Postfix()
        {
            var exists = Configuration.TryGetSettings<Dummy>(out var settings);

            exists.Should().BeTrue();
            settings.Should().NotBeNull();
        }

        [TestMethod]
        public void Should_Be_Able_To_Fetch_Settings_Directly_By_Typename_With_Settings_Postfix()
        {
            var exists = Configuration.TryGetSettings<DummySettings>(out var settings);

            exists.Should().BeTrue();
            settings.Should().NotBeNull();
        }

        [TestMethod]
        public void Should_Be_Able_To_Fetch_Settings_Directly_By_Given_String_Path()
        {
            var exists = Configuration.TryGetSettings<DummySettings>(nameof(Dummy), out var settings);

            exists.Should().BeTrue();
            settings.Should().NotBeNull();
        }

        [TestMethod]
        public void Should_Not_Be_Able_To_Fetch_Settings_If_Settings_Does_Not_Exists()
        {
            var exists = Configuration.TryGetSettings<Person>(out var settings);

            exists.Should().BeFalse();
            settings.Should().NotBeNull();
        }

        [TestMethod]
        public void Should_Throw_Missing_Settings_Exception_When_Settings_Does_Not_Exists()
        {
            var exists = Configuration.TryGetSettings<DummySettings>(out var settings);
            exists.Should().Be(true);

            var expectedResult = new DummySettings { Value0 = "A", Value1 = "B", Value2 = "C" };

            Assert.That.ObjectsAreEqual(() => settings, () => expectedResult);
        }
    }
}

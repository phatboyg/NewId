namespace NewId.Tests
{
    using NUnit.Framework;
    using Providers;

    [TestFixture]
    public class When_getting_a_network_address_for_the_id_generator
    {
        [Test]
        public void Should_pull_the_network_adapter_mac_address()
        {
            var networkIdProvider = new DefaultNetworkIdProvider();

            byte[] networkId = networkIdProvider.NetworkId;

            Assert.IsNotNull(networkId);
            Assert.AreEqual(6, networkId.Length);
        }
    }
}
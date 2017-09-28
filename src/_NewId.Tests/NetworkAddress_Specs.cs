namespace MassTransit.NewIdTests
{
    using System;
    using System.Linq;
    using System.Management;
    using System.Net.NetworkInformation;
    using NewIdProviders;
    using NUnit.Framework;


    [TestFixture]
    public class When_getting_a_network_address_for_the_id_generator
    {
        [Test, Explicit]
        public void Should_match_perhaps()
        {
            var networkAddressWorkerIdProvider = new NetworkAddressWorkerIdProvider();

            byte[] firstId = networkAddressWorkerIdProvider.GetWorkerId(0);

            var wmiNetworkAddressWorkerIdProvider = new WmiNetworkAddressWorkerIdProvider();

            byte[] secondId = wmiNetworkAddressWorkerIdProvider.GetWorkerId(2);


            Assert.AreEqual(firstId, secondId);
        }

        [Test]
        public void Should_pull_all_adapters()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .ToArray();

            foreach (NetworkInterface networkInterface in interfaces)
            {
                Console.WriteLine("Network Interface: {0} - {1}", networkInterface.Description,
                    networkInterface.NetworkInterfaceType);
            }
        }

        [Test]
        public void Should_pull_the_network_adapter_mac_address()
        {
            var networkIdProvider = new NetworkAddressWorkerIdProvider();

            byte[] networkId = networkIdProvider.GetWorkerId(0);

            Assert.IsNotNull(networkId);
            Assert.AreEqual(6, networkId.Length);
        }

        [Test]
        public void Should_pull_the_network_from_wmi()
        {
            var networkIdProvider = new WmiNetworkAddressWorkerIdProvider();

            byte[] networkId = networkIdProvider.GetWorkerId(0);

            Assert.IsNotNull(networkId);
            Assert.AreEqual(6, networkId.Length);
        }

        [Test]
        public void Should_pull_using_WMI()
        {
            var options = new EnumerationOptions {Rewindable = false, ReturnImmediately = true};
            var scope = new ManagementScope(ManagementPath.DefaultPath);
            var query = new ObjectQuery("SELECT * FROM Win32_NetworkAdapter");

            var searcher = new ManagementObjectSearcher(scope, query, options);
            ManagementObjectCollection collection = searcher.Get();
            foreach (ManagementObject obj in collection)
            {
                try
                {
                    PropertyData typeData = obj.Properties["AdapterType"];
                    string typeValue = typeData.Value.ToString();

                    PropertyData propertyData = obj.Properties["MACAddress"];
                    string propertyValue = propertyData.Value.ToString();

                    Console.WriteLine("Adapter: {0}-{1}", propertyValue, typeValue);
                }
                catch (Exception ex)
                {
                }
            }
        }

        [Test]
        public void Should_pull_using_host_name()
        {
            var networkIdProvider = new HostNameHashWorkerIdProvider();

            byte[] networkId = networkIdProvider.GetWorkerId(0);

            Assert.IsNotNull(networkId);
            Assert.AreEqual(6, networkId.Length);
        }

        [Test]
        public void Should_pull_using_net()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                            || x.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet
                            || x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                            || x.NetworkInterfaceType == NetworkInterfaceType.FastEthernetFx
                            || x.NetworkInterfaceType == NetworkInterfaceType.FastEthernetT
                ).ToArray();

            foreach (NetworkInterface networkInterface in interfaces)
                Console.WriteLine("Network Interface: {0}", networkInterface.Description);
        }
    }
}
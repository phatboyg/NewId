namespace NewId.Tests
{
    using System;
    using System.Net.NetworkInformation;
    using NUnit.Framework;

    [TestFixture]
    public class When_getting_a_network_address_for_the_id_generator
    {
        [Test]
        public void Should_pull_the_network_adapter_mac_address()
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                Console.WriteLine(adapter.Description);
                Console.WriteLine("  DNS suffix .............................. : {0}",
                                  properties.DnsSuffix);
                Console.WriteLine("  DNS enabled ............................. : {0}",
                                  properties.IsDnsEnabled);
                Console.WriteLine("  Dynamically configured DNS .............. : {0}",
                                  properties.IsDynamicDnsEnabled);

                Console.WriteLine("  Network Type: ........................... : {0}", adapter.NetworkInterfaceType);
                Console.WriteLine("  Network Address: ........................ : {0}",
                                  adapter.GetPhysicalAddress());
            }
            Console.WriteLine();
        }
    }
}
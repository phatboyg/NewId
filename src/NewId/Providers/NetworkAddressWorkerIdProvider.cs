namespace NewId.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.NetworkInformation;

    public class NetworkAddressWorkerIdProvider :
        WorkerIdProvider
    {
        public byte[] GetWorkerId(int index)
        {
            return GetNetworkAddress(index);
        }

        static byte[] GetNetworkAddress(int index)
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            IEnumerable<NetworkInterface> ethernet =
                interfaces.Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Ethernet);
            IEnumerable<NetworkInterface> gigabit =
                interfaces.Where(x => x.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet);
            IEnumerable<NetworkInterface> wireless =
                interfaces.Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);

            NetworkInterface network = ethernet.Concat(gigabit).Concat(wireless)
                .Skip(index)
                .FirstOrDefault();

            if (network == null)
            {
                throw new InvalidOperationException("Unable to find usable network adapter for unique address");
            }

            byte[] address = network.GetPhysicalAddress().GetAddressBytes();
            return address;
        }
    }
}
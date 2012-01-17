namespace NewId.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.NetworkInformation;

    public class DefaultNetworkIdProvider :
        NetworkIdProvider
    {
        byte[] _address;

        public byte[] NetworkId
        {
            get { return _address ?? (_address = GetNetworkAddress()); }
        }

        static byte[] GetNetworkAddress()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            IEnumerable<NetworkInterface> ethernet =
                interfaces.Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Ethernet);
            IEnumerable<NetworkInterface> gigabit =
                interfaces.Where(x => x.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet);
            IEnumerable<NetworkInterface> wireless =
                interfaces.Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);

            NetworkInterface network = ethernet.Concat(gigabit).Concat(wireless).FirstOrDefault();
            if (network == null)
            {
                throw new InvalidOperationException("Unable to find usable network adapter for unique address");
            }

            byte[] address = network.GetPhysicalAddress().GetAddressBytes();
            return address;
        }
    }
}
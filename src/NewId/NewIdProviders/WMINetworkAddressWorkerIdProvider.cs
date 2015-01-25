namespace MassTransit.NewIdProviders
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management;


    public class WmiNetworkAddressWorkerIdProvider :
        IWorkerIdProvider
    {
        public byte[] GetWorkerId(int index)
        {
            return GetNetworkAddress(index);
        }

        static byte[] GetNetworkAddress(int index)
        {
            byte[] network = GetManagementObjects()
                .Skip(index)
                .FirstOrDefault();

            if (network == null)
                throw new InvalidOperationException("Unable to find usable network adapter for unique address");

            return network;
        }

        static IEnumerable<byte[]> GetManagementObjects()
        {
            var options = new EnumerationOptions {Rewindable = false, ReturnImmediately = true};
            var scope = new ManagementScope(ManagementPath.DefaultPath);
            var query = new ObjectQuery("SELECT * FROM Win32_NetworkAdapter");

            var searcher = new ManagementObjectSearcher(scope, query, options);
            ManagementObjectCollection collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                byte[] bytes;
                try
                {
                    PropertyData propertyData = obj.Properties["MACAddress"];
                    string propertyValue = propertyData.Value.ToString();

                    bytes = propertyValue.Split(':')
                        .Select(x => byte.Parse(x, NumberStyles.HexNumber))
                        .ToArray();
                }
                catch (Exception)
                {
                    continue;
                }

                if (bytes.Length == 6)
                    yield return bytes;
            }
        }
    }
}
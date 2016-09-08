namespace MassTransit.NewIdProviders
{
    using System;
    using System.Collections.Generic;


    public class BestPossibleWorkerIdProvider :
        IWorkerIdProvider
    {
        public byte[] GetWorkerId(int index)
        {
            var exceptions = new List<Exception>();

            try
            {
                return new NetworkAddressWorkerIdProvider().GetWorkerId(index);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

#if !NETCORE
            try
            {
                return new WmiNetworkAddressWorkerIdProvider().GetWorkerId(index);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
#endif

            try
            {
                return new HostNameHashWorkerIdProvider().GetWorkerId(index);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            throw new AggregateException(exceptions);
        }
    }
}
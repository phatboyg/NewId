using System;
using System.Threading;
using MassTransit.NewIdProviders;
using NUnit.Framework;

namespace MassTransit.NewIdTests
{
    [TestFixture]
    public class StopwatchTickProvider_Specs
    {
        [Test, Explicit]
        public void Should_not_lag_time()
        {
            TimeSpan timeDelta = TimeSpan.FromMinutes(1);

            StopwatchTickProvider startProvider = new StopwatchTickProvider();
            Thread.Sleep(timeDelta);
            StopwatchTickProvider endProvider = new StopwatchTickProvider();

            long deltaTicks = Math.Abs(endProvider.Ticks - startProvider.Ticks);
            // 0.01% acceptable delta
            long acceptableDelta = (long)(timeDelta.Ticks * 0.0001);

            Assert.Less(deltaTicks, acceptableDelta);
        }
    }
}
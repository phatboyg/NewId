namespace NewId.Tests
{
    using System;
    using NUnit.Framework;
    using Util;

    [TestFixture]
    public class Generating_ids_over_time
    {
        [Test]
        public void Should_keep_them_ordered_for_sql_server()
        {
            var generator = new NewIdGenerator(new NetworkId().GetPhysicalNetworkId(), new TimeLapseTickProvider());
            generator.Next();

            int limit = 1024;

            var ids = new NewId[limit];
            for (int i = 0; i < limit; i++)
            {
                ids[i] = generator.Next();
            }

            for (int i = 0; i < limit - 1; i++)
            {
                Assert.AreNotEqual(ids[i], ids[i + 1]);
                if (i%16 == 0)
                    Console.WriteLine(ids[i]);
            }
        }

        class TimeLapseTickProvider :
            ITickProvider
        {
            DateTime _previous = DateTime.UtcNow;

            public long Ticks
            {
                get
                {
                    _previous = _previous.AddDays(1).AddHours(7).AddMinutes(22).AddSeconds(31);
                    return _previous.Ticks;
                }
            }
        }
    }
}
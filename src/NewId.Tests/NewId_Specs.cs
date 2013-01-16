namespace NewId.Tests
{
    using System;
    using System.Diagnostics;
    using NUnit.Framework;
    using Providers;

    [TestFixture]
    public class Using_the_newid_generator
    {
        [Test]
        public void Should_generate_sequential_ids_quickly()
        {
            NewId.SetTickProvider(new StopwatchTickProvider());
            NewId.Next();

            int limit = 10;

            var ids = new NewId[limit];
            for (int i = 0; i < limit; i++)
            {
                ids[i] = NewId.Next();
            }

            for (int i = 0; i < limit - 1; i++)
            {
                Assert.AreNotEqual(ids[i], ids[i + 1]);
                Console.WriteLine(ids[i]);
            }
        }

        [Test, Explicit]
        public void Should_be_able_to_extract_timestamp()
        {
            DateTime now = DateTime.UtcNow;
            NewId id = NewId.Next();

            DateTime timestamp = id.Timestamp;

            Console.WriteLine("Now: {0}, Timestamp: {1}", now, timestamp);

            var difference = (timestamp - now);
            if (difference < TimeSpan.Zero)
                difference = difference.Negate();

            Assert.LessOrEqual(difference, TimeSpan.FromMinutes(1));
        }

        [Test, Explicit]
        public void Should_generate_unique_identifiers_with_each_invocation()
        {
            NewId.Next();

            Stopwatch timer = Stopwatch.StartNew();

            int limit = 1024*1024;

            var ids = new NewId[limit];
            for (int i = 0; i < limit; i++)
            {
                ids[i] = NewId.Next();
            }

            timer.Stop();

            for (int i = 0; i < limit - 1; i++)
            {
                Assert.AreNotEqual(ids[i], ids[i + 1]);
                string end = ids[i].ToString().Substring(32, 4);
                if (end == "0000")
                {
                    Console.WriteLine("{0}", ids[i].ToString());
                }
            }

            Console.WriteLine("Generated {0} ids in {1}ms ({2}/ms)", limit, timer.ElapsedMilliseconds,
                limit/timer.ElapsedMilliseconds);
        }
    }
}
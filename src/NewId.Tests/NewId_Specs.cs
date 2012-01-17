namespace NewId.Tests
{
    using System;
    using System.Diagnostics;
    using NUnit.Framework;
    using Util;

    [TestFixture]
    public class Using_the_newid_generator
    {
        [Test]
        public void Should_generate_unique_identifiers_with_each_invocation()
        {
            var generator = new NewIdGenerator(new NetworkId().GetPhysicalNetworkId(), new DateTimeTickProvider());

            generator.Next();

            var timer = Stopwatch.StartNew();

            int limit = 1024 * 1024;

            NewId[] ids = new NewId[limit];
            for (int i = 0; i < limit; i++)
            {
                ids[i] = generator.Next();
            }

            timer.Stop();

            for (int i = 0; i < limit - 1; i++)
            {
                Assert.AreNotEqual(ids[i], ids[i + 1]);
                if(i%1024 == 0)
                    Console.WriteLine(ids[i]);
            }

            Console.WriteLine("Generated {0} ids in {1}ms", limit, timer.ElapsedMilliseconds);
        }

        [Test]
        public void Should_generate_sequential_ids_quickly()
        {
            var generator = new NewIdGenerator(new NetworkId().GetPhysicalNetworkId(), new DateTimeTickProvider());
            generator.Next();

            int limit = 10;

            NewId[] ids = new NewId[limit];
            for (int i = 0; i < limit; i++)
            {
                ids[i] = generator.Next();
            }

            for (int i = 0; i < limit - 1; i++)
            {
                Assert.AreNotEqual(ids[i], ids[i + 1]);
                Console.WriteLine(ids[i]);
            }
        }
    }
}
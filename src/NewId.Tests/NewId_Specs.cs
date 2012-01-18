namespace NewId.Tests
{
    using System;
    using System.Diagnostics;
    using NUnit.Framework;

    [TestFixture]
    public class Using_the_newid_generator
    {
        [Test]
        public void Should_generate_unique_identifiers_with_each_invocation()
        {
            NewId.Next();

            var timer = Stopwatch.StartNew();

            int limit = 1024 * 1024;

            NewId[] ids = new NewId[limit];
            for (int i = 0; i < limit; i++)
            {
                ids[i] = NewId.Next();
            }

            timer.Stop();

            for (int i = 0; i < limit - 1; i++)
            {
                Assert.AreNotEqual(ids[i], ids[i + 1]);
            }

            Console.WriteLine("Generated {0} ids in {1}ms ({2}/ms)", limit, timer.ElapsedMilliseconds,
                              limit/timer.ElapsedMilliseconds);
        }

        [Test]
        public void Should_generate_sequential_ids_quickly()
        {
            NewId.Next();

            int limit = 10;

            NewId[] ids = new NewId[limit];
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
    }
}
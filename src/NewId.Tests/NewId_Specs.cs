namespace MassTransit.NewIdTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NewIdProviders;
    using NUnit.Framework;


    [TestFixture]
    public class Using_the_newid_generator
    {
        [Test, Explicit]
        public void Should_be_able_to_extract_timestamp()
        {
            DateTime now = DateTime.UtcNow;
            NewId id = NewId.Next();

            DateTime timestamp = id.Timestamp;

            Console.WriteLine("Now: {0}, Timestamp: {1}", now, timestamp);

            TimeSpan difference = (timestamp - now);
            if (difference < TimeSpan.Zero)
                difference = difference.Negate();

            Assert.LessOrEqual(difference, TimeSpan.FromMinutes(1));
        }

        [Test]
        public void Should_be_able_to_determine_equal_ids()
        {
            var id1 = new NewId("fc070000-9565-3668-e000-08d5893343c6");
            var id2 = new NewId("fc070000-9565-3668-e000-08d5893343c6");

            Assert.IsTrue(id1 == id2);
        }

        [Test]
        public void Should_be_able_to_determine_greater_id()
        {
            var lowerId = new NewId("fc070000-9565-3668-e000-08d5893343c6");
            var greaterId = new NewId("fc070000-9565-3668-9180-08d589338b38");

            Assert.IsTrue(lowerId < greaterId);
        }

        [Test]
        public void Should_be_able_to_determine_lower_id()
        {
            var lowerId = new NewId("fc070000-9565-3668-e000-08d5893343c6");
            var greaterId = new NewId("fc070000-9565-3668-9180-08d589338b38");

            Assert.IsFalse(lowerId > greaterId);
        }

        [Test, Explicit]
        public void Should_be_able_to_extract_timestamp_with_process_id()
        {
            DateTime now = DateTime.UtcNow;
            NewId.SetProcessIdProvider(new CurrentProcessIdProvider());
            NewId id = NewId.Next();

            DateTime timestamp = id.Timestamp;

            Console.WriteLine("Now: {0}, Timestamp: {1}", now, timestamp);

            TimeSpan difference = (timestamp - now);
            if (difference < TimeSpan.Zero)
                difference = difference.Negate();

            Assert.LessOrEqual(difference, TimeSpan.FromMinutes(1));
        }

        [Test]
        public void Should_generate_sequential_ids_quickly()
        {
            NewId.SetTickProvider(new StopwatchTickProvider());
            NewId.Next();

            int limit = 10;

            var ids = new NewId[limit];
            for (int i = 0; i < limit; i++)
                ids[i] = NewId.Next();

            for (int i = 0; i < limit - 1; i++)
            {
                Assert.AreNotEqual(ids[i], ids[i + 1]);
                Console.WriteLine(ids[i]);
            }
        }

        [Test, Explicit]
        public void Should_generate_unique_identifiers_with_each_invocation()
        {
            NewId.Next();

            Stopwatch timer = Stopwatch.StartNew();

            int limit = 1024 * 1024;

            var ids = new NewId[limit];
            for (int i = 0; i < limit; i++)
                ids[i] = NewId.Next();

            timer.Stop();

            for (int i = 0; i < limit - 1; i++)
            {
                Assert.AreNotEqual(ids[i], ids[i + 1]);
                string end = ids[i].ToString().Substring(32, 4);
                if (end == "0000")
                    Console.WriteLine("{0}", ids[i].ToString());
            }

            Console.WriteLine("Generated {0} ids in {1}ms ({2}/ms)", limit, timer.ElapsedMilliseconds,
                limit / timer.ElapsedMilliseconds);
        }

        [Test, Explicit]
        public void Should_be_completely_thread_safe_to_avoid_duplicates()
        {
            NewId.Next();

            Stopwatch timer = Stopwatch.StartNew();

            int threadCount = 20;

            var loopCount = 1024 * 1024;

            int limit = loopCount * threadCount;

            var ids = new NewId[limit];

            ParallelEnumerable
                .Range(0, limit)
                .WithDegreeOfParallelism(8)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .ForAll(x => { ids[x] = NewId.Next(); });

            timer.Stop();

            Console.WriteLine("Generated {0} ids in {1}ms ({2}/ms)", limit, timer.ElapsedMilliseconds,
                limit / timer.ElapsedMilliseconds);

            Console.WriteLine("Distinct: {0}", ids.Distinct().Count());

            var duplicates = ids.GroupBy(x => x).Where(x => x.Count() > 1).ToArray();

            Console.WriteLine("Duplicates: {0}", duplicates.Count());

            foreach (var newId in duplicates)
            {
                Console.WriteLine("{0} {1}", newId.Key, newId.Count());
            }
        }

        [Test, Explicit]
        public void Should_be_fast_and_friendly()
        {
            NewId.Next();


            int limit = 1000000;

            var ids = new NewId[limit];

            Parallel.For(0, limit, x => ids[x] = NewId.Next());

            Stopwatch timer = Stopwatch.StartNew();

            Parallel.For(0, limit, x => ids[x] = NewId.Next());

            timer.Stop();

            Console.WriteLine("Generated {0} ids in {1}ms ({2}/ms)", limit, timer.ElapsedMilliseconds,
                limit / timer.ElapsedMilliseconds);

            Console.WriteLine("Distinct: {0}", ids.Distinct().Count());

            var duplicates = ids.GroupBy(x => x).Where(x => x.Count() > 1).ToArray();

            Console.WriteLine("Duplicates: {0}", duplicates.Count());

            foreach (var newId in duplicates)
            {
                Console.WriteLine("{0} {1}", newId.Key, newId.Count());
            }
        }
    }
}

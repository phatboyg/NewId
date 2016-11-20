namespace MassTransit.NewIdTests
{
    using System;
    using System.Diagnostics;
    using NUnit.Framework;

    [TestFixture]
    public class When_generating_id
    {
        [Test]
        public void Should_match_when_all_providers_equal()
        {
            // Arrange
            var generator1 = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);
            var generator2 = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);

            // Act
            var id1 = generator1.Next();
            var id2 = generator2.Next();

            // Assert
            Assert.AreEqual(id1, id2);
        }

        [Test]
        public void Should_not_match_when_generated_from_two_processes()
        {
            // Arrange
            var generator1 = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);

            _processIdProvider = new MockProcessIdProvider(BitConverter.GetBytes(11));
            var generator2 = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);

            // Act
            var id1 = generator1.Next();
            var id2 = generator2.Next();

            // Assert
            Assert.AreNotEqual(id1, id2);
        }

        [Test]
        public void Should_not_match_when_processor_id_provided()
        {
            // Arrange
            var generator1 = new NewIdGenerator(_tickProvider, _workerIdProvider);
            var generator2 = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);

            // Act
            var id1 = generator1.Next();
            var id2 = generator2.Next();

            // Assert
            Assert.AreNotEqual(id1, id2);
        }

        [SetUp]
        public void Init()
        {
            _start = DateTime.UtcNow;
            _stopwatch = Stopwatch.StartNew();

            _tickProvider = new MockTickProvider(GetTicks());
            _workerIdProvider = new MockNetworkProvider(BitConverter.GetBytes(1234567890L));
            _processIdProvider = new MockProcessIdProvider(BitConverter.GetBytes(10));
        }

        ITickProvider _tickProvider;
        IWorkerIdProvider _workerIdProvider;
        IProcessIdProvider _processIdProvider;
        DateTime _start;
        Stopwatch _stopwatch;

        long GetTicks()
        {
            return _start.AddTicks(_stopwatch.Elapsed.Ticks).Ticks;
        }

        class MockTickProvider :
            ITickProvider
        {
            public MockTickProvider(long ticks)
            {
                Ticks = ticks;
            }

            public long Ticks { get; }
        }

        class MockNetworkProvider :
            IWorkerIdProvider
        {
            readonly byte[] _workerId;

            public MockNetworkProvider(byte[] workerId)
            {
                _workerId = workerId;
            }

            public byte[] GetWorkerId(int index)
            {
                return _workerId;
            }
        }

        class MockProcessIdProvider :
            IProcessIdProvider
        {
            readonly byte[] _processId;

            public MockProcessIdProvider(byte[] processId)
            {
                _processId = processId;
            }

            public byte[] GetProcessId()
            {
                return _processId;
            }
        }
    }
}
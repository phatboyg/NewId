namespace MassTransit.NewIdTests
{
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Diagnostics;

    [TestFixture]
    public class When_generating_id
    {
        Mock<ITickProvider> _tickProviderMock;
        Mock<IWorkerIdProvider> _workerIdProviderMock;
        Mock<IProcessIdProvider> _processIdProviderMock;
        DateTime _start;
        Stopwatch _stopwatch;

        [SetUp]
        public void Init()
        {
            _tickProviderMock = new Mock<ITickProvider>();
            _workerIdProviderMock = new Mock<IWorkerIdProvider>();
            _processIdProviderMock = new Mock<IProcessIdProvider>();
            _start = DateTime.UtcNow;
            _stopwatch = Stopwatch.StartNew();
        }

        private long GetTicks()
        {
            return (_start.AddTicks(_stopwatch.Elapsed.Ticks)).Ticks;
        }

        [Test]
        public void Should_not_match_when_processor_id_provided()
        {
            // Arrange
            var ticks = GetTicks();
            var networkPhysicalAddress = BitConverter.GetBytes(1234567890L);
            var processorId = BitConverter.GetBytes(10);

            _tickProviderMock.SetupGet(x => x.Ticks).Returns(ticks);
            _workerIdProviderMock.Setup(x => x.GetWorkerId(0)).Returns(networkPhysicalAddress);
            _processIdProviderMock.Setup(x => x.GetProcessId()).Returns(processorId);

            var generator1 = new NewIdGenerator(_tickProviderMock.Object, _workerIdProviderMock.Object);
            var generator2 = new NewIdGenerator(_tickProviderMock.Object, _workerIdProviderMock.Object, _processIdProviderMock.Object);

            // Act
            var id1 = generator1.Next();
            var id2 = generator2.Next();

            // Assert
            Assert.AreNotEqual(id1, id2);
            _processIdProviderMock.Verify(x => x.GetProcessId(), Times.Once());
        }

        [Test]
        public void Should_not_match_when_generated_from_two_processes()
        {
            // Arrange
            var ticks = GetTicks();
            var networkPhysicalAddress = BitConverter.GetBytes(1234567890L);
            var processorId = 10;

            _tickProviderMock.SetupGet(x => x.Ticks).Returns(ticks);
            _workerIdProviderMock.Setup(x => x.GetWorkerId(0)).Returns(networkPhysicalAddress);
            _processIdProviderMock.Setup(x => x.GetProcessId()).Returns(() => BitConverter.GetBytes(processorId++));

            var generator1 = new NewIdGenerator(_tickProviderMock.Object, _workerIdProviderMock.Object, _processIdProviderMock.Object);
            var generator2 = new NewIdGenerator(_tickProviderMock.Object, _workerIdProviderMock.Object, _processIdProviderMock.Object);

            // Act
            var id1 = generator1.Next();
            var id2 = generator2.Next();

            // Assert
            Assert.AreNotEqual(id1, id2);
            _processIdProviderMock.Verify(x => x.GetProcessId(), Times.Exactly(2));
        }

        [Test]
        public void Should_match_when_all_providers_equal()
        {
            // Arrange
            var ticks = GetTicks();
            var networkPhysicalAddress = BitConverter.GetBytes(1234567890L);
            var processorId = BitConverter.GetBytes(10);

            _tickProviderMock.SetupGet(x => x.Ticks).Returns(ticks);
            _workerIdProviderMock.Setup(x => x.GetWorkerId(0)).Returns(networkPhysicalAddress);
            _processIdProviderMock.Setup(x => x.GetProcessId()).Returns(processorId);

            var generator1 = new NewIdGenerator(_tickProviderMock.Object, _workerIdProviderMock.Object, _processIdProviderMock.Object);
            var generator2 = new NewIdGenerator(_tickProviderMock.Object, _workerIdProviderMock.Object, _processIdProviderMock.Object);

            // Act
            var id1 = generator1.Next();
            var id2 = generator2.Next();

            // Assert
            Assert.AreEqual(id1, id2);
        }
    }
}
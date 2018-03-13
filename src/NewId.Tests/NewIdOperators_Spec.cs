namespace NewId.Tests
{
    using MassTransit;
    using NUnit.Framework;

    [TestFixture]
    public class Using_the_newid_operators
    {
        [Test, Explicit]
        public void Should_be_able_to_determine_equal_ids()
        {
            var id1 = new NewId("fc070000-9565-3668-e000-08d5893343c6");
            var id2 = new NewId("fc070000-9565-3668-e000-08d5893343c6");

            Assert.IsTrue(id1 == id2);
        }

        [Test, Explicit]
        public void Should_be_able_to_determine_greater_id()
        {
            var lowerId = new NewId("fc070000-9565-3668-e000-08d5893343c6");
            var greaterId = new NewId("fc070000-9565-3668-9180-08d589338b38");

            Assert.IsTrue(lowerId < greaterId);
        }

        [Test, Explicit]
        public void Should_be_able_to_determine_lower_id()
        {
            var lowerId = new NewId("fc070000-9565-3668-e000-08d5893343c6");
            var greaterId = new NewId("fc070000-9565-3668-9180-08d589338b38");

            Assert.IsFalse(lowerId > greaterId);
        }
    }
}

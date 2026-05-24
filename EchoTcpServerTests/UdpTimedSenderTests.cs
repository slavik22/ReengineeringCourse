using EchoTcpServer;

namespace EchoTcpServerTests
{
    [TestFixture]
    public class UdpTimedSenderTests
    {
        [Test]
        public void StartSending_WhenAlreadyRunning_ThrowsInvalidOperationException()
        {
            using var sender = new UdpTimedSender("127.0.0.1", 19999);
            sender.StartSending(60_000);

            Assert.Throws<InvalidOperationException>(() => sender.StartSending(60_000));
        }

        [Test]
        public void StopSending_WithoutStarting_DoesNotThrow()
        {
            using var sender = new UdpTimedSender("127.0.0.1", 19999);

            Assert.DoesNotThrow(() => sender.StopSending());
        }

        [Test]
        public void StopSending_AfterStart_AllowsSecondStart()
        {
            using var sender = new UdpTimedSender("127.0.0.1", 19999);
            sender.StartSending(60_000);
            sender.StopSending();

            Assert.DoesNotThrow(() => sender.StartSending(60_000));
        }

        [Test]
        public void Dispose_WithoutStarting_DoesNotThrow()
        {
            var sender = new UdpTimedSender("127.0.0.1", 19999);

            Assert.DoesNotThrow(() => sender.Dispose());
        }

        [Test]
        public void Dispose_WhileSending_DoesNotThrow()
        {
            var sender = new UdpTimedSender("127.0.0.1", 19999);
            sender.StartSending(60_000);

            Assert.DoesNotThrow(() => sender.Dispose());
        }
    }
}

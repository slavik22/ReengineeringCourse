using NetSdrClientApp.Messages;

namespace NetSdrClientAppTests
{
    public class NetSdrMessageHelperTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetControlItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverState;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetControlItemMessage(type, code, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var codeBytes = msg.Skip(2).Take(2);
            var parametersBytes = msg.Skip(4);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);
            var actualCode = BitConverter.ToInt16(codeBytes.ToArray());

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(actualCode, Is.EqualTo((short)code));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        [Test]
        public void GetDataItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.DataItem2;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetDataItemMessage(type, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var parametersBytes = msg.Skip(2);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        [Test]
        public void TranslateMessageRoundTripTest()
        {
            // arrange — build a SetControlItem message then parse it back
            var parameters = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            var msg = NetSdrMessageHelper.GetControlItemMessage(
                NetSdrMessageHelper.MsgTypes.SetControlItem,
                NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency,
                parameters);

            // act
            bool success = NetSdrMessageHelper.TranslateMessage(msg,
                out var type, out var itemCode, out _, out var body);

            // assert
            Assert.That(success, Is.True);
            Assert.That(type, Is.EqualTo(NetSdrMessageHelper.MsgTypes.SetControlItem));
            Assert.That(itemCode, Is.EqualTo(NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency));
            Assert.That(body, Is.EqualTo(parameters));
        }

        [Test]
        public void GetSamples16BitReturnsCorrectCountTest()
        {
            // 6 bytes → 3 samples of 16 bits
            var body = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };

            var samples = NetSdrMessageHelper.GetSamples(16, body).ToList();

            Assert.That(samples.Count, Is.EqualTo(3));
        }

        [Test]
        public void GetSamplesThrowsForOversizedBitsTest()
        {
            // 40 bits / 8 = 5 bytes > 4 → ArgumentOutOfRangeException
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                NetSdrMessageHelper.GetSamples(40, new byte[8]).ToList());
        }

        //TODO: add more NetSdrMessageHelper tests
    }
}
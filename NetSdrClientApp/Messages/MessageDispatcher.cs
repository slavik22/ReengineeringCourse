using NetSdrClientApp.Networking;

namespace NetSdrClientApp.Messages
{
    // VIOLATION: Messages layer must not depend on Networking layer.
    // This class will be removed in the fix commit.
    internal class MessageDispatcher
    {
        private readonly ITcpClient _tcpClient;

        public MessageDispatcher(ITcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }
    }
}

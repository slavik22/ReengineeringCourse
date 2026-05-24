namespace EchoTcpServer.Networking
{
    public interface ITcpListener
    {
        void Start();
        void Stop();
        Task<Stream> AcceptClientStreamAsync();
    }
}

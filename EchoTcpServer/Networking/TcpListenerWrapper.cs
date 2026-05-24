using System.Net;
using System.Net.Sockets;

namespace EchoTcpServer.Networking;

public class TcpListenerWrapper : ITcpListener
{
    private readonly TcpListener _listener;

    public TcpListenerWrapper(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public void Start() => _listener.Start();
    public void Stop() => _listener.Stop();

    public async Task<Stream> AcceptClientStreamAsync()
    {
        var client = await _listener.AcceptTcpClientAsync();
        return client.GetStream();
    }
}

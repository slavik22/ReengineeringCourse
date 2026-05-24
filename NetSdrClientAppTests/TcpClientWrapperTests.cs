using System.Net;
using System.Net.Sockets;
using NetSdrClientApp.Networking;

namespace NetSdrClientAppTests;

[TestFixture]
public class TcpClientWrapperTests
{
    [Test]
    public void Disconnect_WhenNotConnected_DoesNotThrow()
    {
        var wrapper = new TcpClientWrapper("127.0.0.1", 19399);
        Assert.DoesNotThrow(() => wrapper.Disconnect());
    }

    [Test]
    public async Task SendMessageAsync_WhenNotConnected_ThrowsInvalidOperationException()
    {
        var wrapper = new TcpClientWrapper("127.0.0.1", 19399);
        Assert.ThrowsAsync<InvalidOperationException>(
            () => wrapper.SendMessageAsync(new byte[] { 1 }));
        await Task.CompletedTask;
    }

    [Test]
    public async Task Connect_SendMessage_ReceivesEchoViaEvent()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        try
        {
            var wrapper = new TcpClientWrapper("127.0.0.1", port);
            var acceptTask = listener.AcceptTcpClientAsync();

            wrapper.Connect();
            using var server = await acceptTask;
            var serverStream = server.GetStream();

            var received = new TaskCompletionSource<byte[]>();
            wrapper.MessageReceived += (_, data) => received.TrySetResult(data);

            var sendData = new byte[] { 0xAA, 0xBB, 0xCC };
            await wrapper.SendMessageAsync(sendData);

            var buf = new byte[sendData.Length];
            int n = await serverStream.ReadAsync(buf.AsMemory());
            await serverStream.WriteAsync(buf.AsMemory(0, n));

            var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(2));
            Assert.That(result, Is.EqualTo(sendData));

            wrapper.Disconnect();
        }
        finally
        {
            listener.Stop();
        }
    }
}

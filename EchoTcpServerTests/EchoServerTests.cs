using Moq;
using EchoTcpServer;
using EchoTcpServer.Networking;

namespace EchoTcpServerTests;

[TestFixture]
public class EchoServerTests
{
    private Mock<ITcpListener> _listenerMock = null!;
    private List<string> _logs = null!;
    private EchoServer _server = null!;

    [SetUp]
    public void SetUp()
    {
        _listenerMock = new Mock<ITcpListener>();
        _logs = new List<string>();
        _server = new EchoServer(_listenerMock.Object, msg => _logs.Add(msg));
    }

    [Test]
    public void Stop_StopsListenerAndLogs()
    {
        _server.Stop();

        _listenerMock.Verify(l => l.Stop(), Times.Once);
        Assert.That(_logs, Contains.Item("Server stopped."));
    }

    [Test]
    public async Task StartAsync_StartsListenerAndLogsShutdown()
    {
        _listenerMock
            .Setup(l => l.AcceptClientStreamAsync())
            .ThrowsAsync(new ObjectDisposedException("listener"));

        await _server.StartAsync();

        _listenerMock.Verify(l => l.Start(), Times.Once);
        Assert.That(_logs, Contains.Item("Server started."));
        Assert.That(_logs, Contains.Item("Server shutdown."));
    }

    [Test]
    public async Task StartAsync_AcceptsClientAndLogs()
    {
        _listenerMock.SetupSequence(l => l.AcceptClientStreamAsync())
            .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }))
            .ThrowsAsync(new ObjectDisposedException("listener"));

        await _server.StartAsync();

        _listenerMock.Verify(l => l.AcceptClientStreamAsync(), Times.Exactly(2));
        Assert.That(_logs, Contains.Item("Client connected."));
    }

    [Test]
    public async Task HandleClientAsync_EchoesInputToOutput()
    {
        var inputData = new byte[] { 0x41, 0x42, 0x43 }; // "ABC"
        var inputStream = new MemoryStream(inputData);
        var outputStream = new MemoryStream();
        var stream = new DuplexStream(inputStream, outputStream);

        await _server.HandleClientAsync(stream, CancellationToken.None);

        Assert.That(outputStream.ToArray(), Is.EqualTo(inputData));
    }

    [Test]
    public async Task HandleClientAsync_LogsEchoedBytes()
    {
        var stream = new DuplexStream(new MemoryStream(new byte[] { 1, 2 }), new MemoryStream());

        await _server.HandleClientAsync(stream, CancellationToken.None);

        Assert.That(_logs.Any(m => m.StartsWith("Echoed")), Is.True);
        Assert.That(_logs, Contains.Item("Client disconnected."));
    }

    [Test]
    public async Task HandleClientAsync_PreCancelledToken_DoesNotRead()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var mockStream = new Mock<Stream>();
        mockStream.Setup(s => s.CanRead).Returns(true);

        await _server.HandleClientAsync(mockStream.Object, cts.Token);

        mockStream.Verify(
            s => s.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task HandleClientAsync_StreamException_LogsError()
    {
        var mockStream = new Mock<Stream>();
        mockStream
            .Setup(s => s.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Connection reset by peer"));

        await _server.HandleClientAsync(mockStream.Object, CancellationToken.None);

        Assert.That(_logs.Any(m => m.StartsWith("Error:")), Is.True);
    }

    [Test]
    public async Task HandleClientAsync_EmptyStream_LogsDisconnected()
    {
        var emptyStream = new DuplexStream(new MemoryStream(Array.Empty<byte>()), new MemoryStream());

        await _server.HandleClientAsync(emptyStream, CancellationToken.None);

        Assert.That(_logs, Contains.Item("Client disconnected."));
    }
}

/// <summary>
/// Reads from one stream, writes to another — simulates a bidirectional client connection.
/// </summary>
internal sealed class DuplexStream : Stream
{
    private readonly Stream _read;
    private readonly Stream _write;

    public DuplexStream(Stream read, Stream write)
    {
        _read = read;
        _write = write;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush() => _write.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _read.Read(buffer, offset, count);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        => _read.ReadAsync(buffer, offset, count, ct);

    public override void Write(byte[] buffer, int offset, int count) => _write.Write(buffer, offset, count);

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        => _write.WriteAsync(buffer, offset, count, ct);

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}

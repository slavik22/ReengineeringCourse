using EchoTcpServer.Networking;

namespace EchoTcpServer
{
    public class EchoServer
    {
        private readonly ITcpListener _listener;
        private readonly Action<string> _log;
        private readonly CancellationTokenSource _cts = new();

        public EchoServer(ITcpListener listener, Action<string>? log = null)
        {
            _listener = listener;
            _log = log ?? Console.WriteLine;
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _log("Server started.");

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var stream = await _listener.AcceptClientStreamAsync();
                    _log("Client connected.");
                    _ = Task.Run(() => HandleClientAsync(stream, _cts.Token));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }

            _log("Server shutdown.");
        }

        public async Task HandleClientAsync(Stream stream, CancellationToken token)
        {
            using (stream)
            {
                try
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while (!token.IsCancellationRequested
                        && (bytesRead = await stream.ReadAsync(buffer.AsMemory(), token)) > 0)
                    {
                        await stream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                        _log($"Echoed {bytesRead} bytes.");
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _log($"Error: {ex.Message}");
                }
            }

            _log("Client disconnected.");
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
            _cts.Dispose();
            _log("Server stopped.");
        }
    }
}

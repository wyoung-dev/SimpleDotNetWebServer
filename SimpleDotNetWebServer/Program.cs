using System.Net;
using System.Net.Sockets;
using System.Text;
using Serilog;

// Demonstrates how to create a simple web server using the .NET implementation of Berkley sockets.
class Program
{
    public static async Task Main(string[] args)
    {
        // Setup Serilog.
        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .CreateLogger();

        string ipAddress;
        int port;

        if (args.Length == 0)
        {
            ipAddress = "127.0.0.1";
            port = 80;
        }
        else
        {
            ipAddress = args[0];
            port = int.Parse(args[1]);
        }

        // Setup the listener/server socket using the IP address family, socket type 'stream' for reliable, two-way streaming, and 'tcp' protocol type.
        using var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the given IP address and port.
        serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
        serverSocket.Listen();

        Log.Information("""
                Listening at: {prefix}{endpoint}:{port}
                Press Ctrl+C to exit.

                """, "http://", ipAddress, port);

        const int bufferSize = 1024;

        // Enter a continuous loop to accept incoming connections.
        while (true)
        {
            var _ = await serverSocket.AcceptAsync()
                .ConfigureAwait(false);

            // Start a new task for each client.
            await Task.Factory.StartNew(async state =>
            {
                if (state is not Socket clientSocket) return;

                // NOTE: Data must be received before a reply can be sent.
                var bytesRead = 0;
                var requestBuffer = new byte[bufferSize];
                using var requestStream = new MemoryStream();

                do
                {
                    bytesRead = await clientSocket.ReceiveAsync(requestBuffer.AsMemory(0, bufferSize))
                        .ConfigureAwait(false);
                    await requestStream.WriteAsync(requestBuffer.AsMemory(0, bytesRead))
                        .ConfigureAwait(false);
                }
                while (bytesRead == bufferSize);

                var request = Encoding.UTF8.GetString(requestStream.ToArray());

                // Log the request.
                Log.Debug("""

            {request}
            """, request);

                // Send the reply.
                // NOTE: New line is expected after the HTTP response code and the response payload.
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("""
                HTTP/1.1 200 OK

                Hello World!
                """)).ConfigureAwait(false);

                // It's important to close and dispose of the incoming socket.
                clientSocket.Close();
                clientSocket.Dispose();
            }, _);
        }
    }
}

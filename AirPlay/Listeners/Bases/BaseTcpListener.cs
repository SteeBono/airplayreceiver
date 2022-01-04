using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AirPlay.Models;
using AirPlay.Models.Enums;

namespace AirPlay.Listeners
{
    public class BaseTcpListener : BaseListener
    {
        private readonly ushort _port;
        private readonly TcpListener _listener;
        private readonly ConcurrentDictionary<string, Task> _connections;
        private readonly bool _rawData = false;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public BaseTcpListener(ushort port, bool rawData = false)
        {
            _port = port;
            _rawData = rawData;
            _listener = new TcpListener(IPAddress.Any, _port);
            _connections = new ConcurrentDictionary<string, Task>();

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);

            Task.Run(() => AcceptClientsAsync(source.Token), source.Token);
            return Task.CompletedTask;
        }

        public override Task StopAsync()
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        public virtual Task OnDataReceivedAsync(Request request, Response response, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnRawReceivedAsync(TcpClient client, NetworkStream stream, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            _listener.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                var task = HandleClientAsync(client, cancellationToken);

                var remoteEndpoint = client.Client.RemoteEndPoint.ToString();
                if (!_connections.TryAdd(remoteEndpoint, task))
                {
                    client.Close();
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            try
            {
                var remoteEndpoint = client.Client.RemoteEndPoint.ToString();
                var stream = client.GetStream();
                Console.WriteLine($"Client connected: {remoteEndpoint}");

                if (_rawData)
                {
                    await OnRawReceivedAsync(client, stream, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await ReadFormattedAsync(client, stream, cancellationToken).ConfigureAwait(false);
                }

                if (client.Connected)
                {
                    client.Close();
                }

                Console.WriteLine($"Client disconnected: {remoteEndpoint}");

                _connections.Remove(remoteEndpoint, out _);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task ReadFormattedAsync(TcpClient client, NetworkStream stream, CancellationToken cancellationToken)
        {
            if (client.Connected && stream.CanRead)
            {
                int retBytes = 0;
                string raw = string.Empty;

                do
                {
                    try
                    {
                        var buffer = new byte[1024];
                        retBytes += stream.Read(buffer, 0, buffer.Length);
                        raw += string.Join(string.Empty, buffer.Take(retBytes).Select(b => b.ToString("X2")));

                        // Wait for other possible data
                        await Task.Delay(10);
                    }
                    catch (System.IO.IOException) { }
                } while (client.Connected && stream.DataAvailable);

                // Now we have all data inside raw var
                // Make sure the socket is still connected (if it closed we don't really care about the message because we can not reply)
                if (client.Connected)
                {
                    // That listener can accept only this methods: GET | POST | SETUP | GET_PARAMETER | RECORD | SET_PARAMETER | ANNOUNCE | FLUSH | TEARDOWN | OPTIONS | PAUSE
                    // Because of the persistent connection we might receive more than one request at a time
                    // I'm using a regex to find all request by 'magic numbers' (ex. GET, POST, SETUP, ecc)

                    var pattern =
                        $"^{RequestConst.GET}[.]*|" +
                        $"^{RequestConst.POST}[.]*|" +
                        $"^{RequestConst.SETUP}[.]*|" +
                        $"^{RequestConst.GET_PARAMETER}[.]*|" +
                        $"^{RequestConst.RECORD}[.]*|" +
                        $"^{RequestConst.SET_PARAMETER}[.]*|" +
                        $"^{RequestConst.ANNOUNCE}[.]*|" +
                        $"^{RequestConst.FLUSH}[.]*|" +
                        $"^{RequestConst.OPTIONS}[.]*|" +
                        $"^{RequestConst.PAUSE}[.]*|" +
                        $"^{RequestConst.TEARDOWN}[.]*";

                    var r = new Regex(pattern, RegexOptions.Multiline);
                    var m = r.Matches(raw);

                    // Split requests and create models
                    var requests = new List<Request>();
                    for (int i = 0; i < m.Count; i++)
                    {
                        if (i + 1 < m.Count)
                        {
                            var hexReq = raw.Substring(m[i].Index, m[i + 1].Index - m[i].Index);
                            var req = new Request(hexReq);
                            requests.Add(req);
                        }
                        else
                        {
                            var hexReq = raw.Substring(m[i].Index);
                            var req = new Request(hexReq);
                            requests.Add(req);
                        }
                    }

                    foreach (var request in requests)
                    {
                        var response = request.GetBaseResponse();

                        await OnDataReceivedAsync(request, response, cancellationToken).ConfigureAwait(false);
                        await SendResponseAsync(stream, response);
                    }
                }

                // If we have read some bytes, leave connection open and wait for next message
                if (retBytes != 0)
                {
                    await ReadFormattedAsync(client, stream, cancellationToken);
                }
            }
        }

        private async Task SendResponseAsync(NetworkStream stream, Response response)
        {
            var format = $"{response.GetProtocol()} {(int)response.StatusCode} {response.StatusCode.ToString().ToUpperInvariant()}\r\n";

            foreach (var header in response.Headers)
            {
                format += $"{header.Name}: {string.Join(",", header.Values)}\r\n";
            }

            // Ready for body
            format += "\r\n";

            var formatBuffer = Encoding.ASCII.GetBytes(format);

            byte[] payload;
            var bodyBuffer = await response.ReadAsync();
            if (bodyBuffer?.Any() == true)
            {
                payload = formatBuffer.Concat(bodyBuffer).ToArray();
            }
            else
            {
                payload = formatBuffer;
            }

            try
            {
                stream.Write(payload, 0, payload.Length);
                stream.Flush();
            }
            catch (System.IO.IOException e)
            {
                // 
            }
        }
    }
}

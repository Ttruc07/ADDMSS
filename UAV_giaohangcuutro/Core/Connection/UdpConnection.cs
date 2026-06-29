using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace UAV_giaohangcuutro.Core.Connection
{
    public class UdpConnection : IDisposable
    {
        private UdpClient? _udpClient;
        private CancellationTokenSource? _cts;
        private bool _isListening;
        private IPEndPoint? _lastRemoteEP;

        public event Action<byte[]>? OnDataReceived;
        public event Action<string>? OnLogMessage;

        public bool IsListening => _isListening;
        public IPEndPoint? LastRemoteEP => _lastRemoteEP;

        public void SendData(byte[] data)
        {
            if (_udpClient == null) return;
            
            // If we don't have a remote endpoint yet, we cannot send
            var target = _lastRemoteEP;
            if (target == null)
            {
                OnLogMessage?.Invoke("Cannot send data: No active telemetry remote endpoint registered yet.");
                return;
            }

            try
            {
                _udpClient.Send(data, data.Length, target);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Failed to send UDP data to {target}: {ex.Message}");
            }
        }

        public void Start(int port)
        {
            if (_isListening) return;

            try
            {
                _udpClient = new UdpClient();

                // FIX 1 (Kinh nghiệm thực chiến): Cho phép tái sử dụng cổng.
                // Tránh lỗi "Only one usage of each socket address" khi app bị tắt
                // và khởi động lại nhanh, hoặc khi nhiều socket cùng lắng nghe.
                _udpClient.Client.SetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.ReuseAddress,
                    true);

                // FIX 2 (Kinh nghiệm thực chiến): Bind vào 127.0.0.1 (Loopback)
                // thay vì 0.0.0.0 (Any) để khớp với địa chỉ Mission Planner Forward
                // và bypass Windows Firewall trong môi trường phát triển.
                _udpClient.Client.Bind(new IPEndPoint(IPAddress.Loopback, port));
                
                _cts = new CancellationTokenSource();
                _isListening = true;

                OnLogMessage?.Invoke($"Started listening on UDP port {port}...");
                
                // Start background receiving loop
                Task.Run(() => ReceiveLoop(_cts.Token));
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Failed to start UDP connection: {ex.Message}");
                Stop();
            }
        }

        public void Stop()
        {
            if (!_isListening) return;

            _isListening = false;
            _cts?.Cancel();
            _udpClient?.Close();
            _udpClient = null;

            OnLogMessage?.Invoke("UDP connection stopped.");
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _isListening && _udpClient != null)
            {
                try
                {
                    // UdpClient.ReceiveAsync respects CancellationToken in modern .NET
                    var result = await _udpClient.ReceiveAsync(token);
                    _lastRemoteEP = result.RemoteEndPoint;
                    if (result.Buffer.Length > 0)
                    {
                        OnDataReceived?.Invoke(result.Buffer);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                    {
                        OnLogMessage?.Invoke($"Error receiving UDP data: {ex.Message}");
                    }
                    // Wait a bit before retrying to avoid CPU hogging in case of persistent errors
                    await Task.Delay(100, token);
                }
            }
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }
}

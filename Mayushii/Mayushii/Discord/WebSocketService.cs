using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discord
{
    internal class WebSocketService
    {
        public event WebSocketServiceReceive OnMessageReceive;

        public event WebSocketServiceUpdate OnStatusUpdate;

        private const int Timeout = 5000;

        private CancellationTokenSource cancellation;

        private string uri;

        private ClientWebSocket websocket;

        public WebSocketService(string uri)
        {
            this.uri = uri;
            websocket = new ClientWebSocket();
            OnMessageReceive += _ => { };
            OnStatusUpdate += _ => { };
        }

        public async void RunService()
        {
            await websocket.ConnectAsync(new Uri(uri), new CancellationTokenSource(Timeout).Token);
            OnStatusUpdate(WebSocketServiceEvent.Connected);
            var buffer = new ArraySegment<byte>(new byte[65525]);
            cancellation = new CancellationTokenSource();
            while (websocket.State == WebSocketState.Open)
            {
                try
                {
                    string message;
                    using (MemoryStream stream = new MemoryStream())
                    {
                        WebSocketReceiveResult result;
                        do
                        {
                            result = await websocket.ReceiveAsync(buffer, cancellation.Token);
                            stream.Write(buffer.Array, buffer.Offset, result.Count);
                        } while (!result.EndOfMessage);
                        stream.Seek(0, SeekOrigin.Begin);
                        message = new StreamReader(stream, Encoding.UTF8).ReadToEnd();
                    }
                    OnMessageReceive(message);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            OnStatusUpdate(WebSocketServiceEvent.Disconnected);
            cancellation.Dispose();
            websocket.Dispose();
        }

        public async Task<bool> Send(string message)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            try
            {
                await websocket.SendAsync(buffer, WebSocketMessageType.Text, true, new CancellationTokenSource(Timeout).Token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            return true;
        }

        public enum WebSocketServiceEvent { Connected, Disconnected };

        public delegate void WebSocketServiceReceive(string message);

        public delegate void WebSocketServiceUpdate(WebSocketServiceEvent updateEvent);
    }
}
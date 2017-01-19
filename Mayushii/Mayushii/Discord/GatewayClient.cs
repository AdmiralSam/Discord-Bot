using Discord.Message;
using Discord.Response;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace Discord
{
    internal class GatewayClient
    {
        public event ConnectedHandler OnConnected;

        public event ReceiveMessageHandler OnReceiveMessage;

        private int heartbeatInterval;

        private Timer heartbeatTimer;

        private JavaScriptSerializer json;

        private int? lastReceived;

        private WebSocketService webService;

        public GatewayClient()
        {
            json = new JavaScriptSerializer();
            OnConnected += () => { };
            OnReceiveMessage += (_) => { };
        }

        public void Initialize()
        {
            HttpWebRequest gatewayUriRequest = WebRequest.CreateHttp("https://discordapp.com/api/gateway");
            WebResponse response = gatewayUriRequest.GetResponse();
            string jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            GatewayGetResponse getResponse = json.Deserialize<GatewayGetResponse>(jsonString);
            webService = new WebSocketService(getResponse.Url + "?v=6&encoding=json");
            webService.OnMessageReceive += WebService_OnMessageReceive;
            webService.OnStatusUpdate += WebService_OnStatusUpdate;
            new Thread(webService.RunService).Start();
        }

        public async void Login()
        {
            await webService.Send(json.Serialize(IdentifyMessage.Mayushii));
        }

        public void SendFile(string channel, Uri url)
        {
            HttpWebRequest getImageRequest = WebRequest.CreateHttp(url);
            getImageRequest.Method = "GET";
            WebResponse response = getImageRequest.GetResponse();
            SendFile(channel, Path.GetFileName(url.LocalPath), response.GetResponseStream());
        }

        public async void SendFile(string channel, string filename, Stream fileStream)
        {
            HttpWebRequest createMessageRequest = WebRequest.CreateHttp($"https://discordapp.com/api/channels/{channel}/messages");
            MultipartFormDataContent content = new MultipartFormDataContent();
            createMessageRequest.ContentType = content.Headers.ContentType.ToString();
            createMessageRequest.Method = "POST";
            createMessageRequest.Headers[HttpRequestHeader.Authorization] = "Bot " + IdentifyMessage.Mayushii.d.token;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                content.Add(new ByteArrayContent(memoryStream.ToArray()), "file", filename);
            }
            await content.CopyToAsync(createMessageRequest.GetRequestStream());
            WebResponse response = createMessageRequest.GetResponse();
            string jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        public void SendMessage(string channel, string message)
        {
            HttpWebRequest createMessageRequest = WebRequest.CreateHttp($"https://discordapp.com/api/channels/{channel}/messages");
            createMessageRequest.Method = "POST";
            createMessageRequest.ContentType = "application/json";
            createMessageRequest.Headers[HttpRequestHeader.Authorization] = "Bot " + IdentifyMessage.Mayushii.d.token;
            string request = json.Serialize(new CreateMessageMessage() { content = message });
            using (Stream requestStream = createMessageRequest.GetRequestStream())
            {
                requestStream.Write(Encoding.UTF8.GetBytes(request), 0, Encoding.UTF8.GetByteCount(request));
            }
            WebResponse response = createMessageRequest.GetResponse();
            string jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        private void WebService_OnMessageReceive(string message)
        {
            GatewayResponse response = json.Deserialize<GatewayResponse>(message);
            if (response == null)
                return;
            switch (response.Op)
            {
                case Op.Hello:
                    HelloResponse hello = json.Deserialize<HelloResponse>(message);
                    heartbeatInterval = hello.D.Heartbeat_Interval;
                    OnConnected();
                    break;

                case Op.Dispatch:
                    DispatchResponse dispatch = json.Deserialize<DispatchResponse>(message);
                    lastReceived = dispatch.S;
                    switch (dispatch.T)
                    {
                        case "READY":
                            heartbeatTimer = new Timer(
                                async (_) => await webService.Send(json.Serialize(new HeartbeatMessage() { d = lastReceived })),
                                null,
                                0,
                                heartbeatInterval
                            );
                            break;

                        case "MESSAGE_CREATE":
                            MessageCreateResponse messageResponse = json.Deserialize<MessageCreateResponse>(message);
                            OnReceiveMessage(messageResponse.D);
                            break;

                        default:
                            Console.WriteLine(message);
                            break;
                    }
                    break;

                default:
                    Console.WriteLine(message);
                    break;
            }
        }

        private void WebService_OnStatusUpdate(WebSocketService.WebSocketServiceEvent updateEvent)
        {
            if (updateEvent == WebSocketService.WebSocketServiceEvent.Disconnected)
            {
                heartbeatTimer.Dispose();
            }
        }

        public delegate void ConnectedHandler();

        public delegate void ReceiveMessageHandler(Objects.Message message);
    }
}
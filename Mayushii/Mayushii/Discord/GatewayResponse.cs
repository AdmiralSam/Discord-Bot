namespace Discord.Response
{
#pragma warning disable 0649

    public enum Op { Dispatch, Heartbeat, Identify, StatusUpdate, VoiceStateUpdate, VoiceServerPing, Resume, Reconnect, RequestGuildMembers, InvalidSession, Hello, HeartbackAck }

    public class DispatchResponse
    {
        public object D;
        public int S;
        public string T;
    }

    public class GatewayGetResponse
    {
        public string Url;
    }

    public class GatewayResponse
    {
        public Op Op;
    }

    public class HelloResponse
    {
        public Payload D;

        public class Payload
        {
            public int Heartbeat_Interval;
        }
    }

    public class MessageCreateResponse
    {
        public Objects.Message D;
    }

#pragma warning restore 0649
}
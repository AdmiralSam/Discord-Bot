using Discord.Response;
using System.Collections.Generic;

namespace Discord.Message
{
    public class CreateMessageMessage
    {
        public string content;
    }

    public class HeartbeatMessage
    {
        public int? d;
        public Op op = Op.Heartbeat;
    }

    public class IdentifyMessage
    {
        public static IdentifyMessage Mayushii = new IdentifyMessage()
        {
            d = new Payload()
            {
                compress = false,
                large_threshold = 250,
                properties = new Dictionary<string, string>
                {
                    { "$os", "windows" },
                    { "$browser", "Mayushii" },
                    { "$device", "Mayushii" },
                    { "$referrer", "" },
                    { "$referring_domain", "" }
                },
                shard = new[] { 0, 1 },
                token = "MjIwOTQ3MjIxMjA4NjI5MjU4.C2GpYw.DzmIh4r3fAvR6vw2-rGdr0SXh2U"
            }
        };

        public Payload d;
        public Op op = Op.Identify;

        public class Payload
        {
            public bool compress;
            public int large_threshold;
            public Dictionary<string, string> properties;
            public int[] shard;
            public string token;
        }
    }
}
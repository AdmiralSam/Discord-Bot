using Discord;
using Mayushii.Services;
using System;
using System.Collections.Generic;

namespace Mayushii
{
    internal class Mayushii
    {
        private GatewayClient client;

        public Mayushii()
        {
            client = new GatewayClient();
            client.Initialize();
            client.OnConnected += client.Login;
            client.OnReceiveMessage += Client_OnReceiveMessage;
        }

        private void Client_OnReceiveMessage(Discord.Objects.Message message)
        {
            if (message.Content.ToLower().Equals("tutturu"))
            {
                client.SendMessage(message.Channel_Id, $"<@{message.Author.Id}> トゥットゥル～♪");
            }
            if (message.Content.ToLower().StartsWith("~show "))
            {
                List<string> tags = new List<string>(message.Content.Split(' '));
                tags.RemoveAt(0);
                string url = DanbooruService.GetRandomImage(tags);
                //client.SendMessage(message.Channel_Id, url ?? string.Format("No Images Found For ({0})", string.Join(", ", tags)));
                if (url != null)
                {
                    client.SendFile(message.Channel_Id, new Uri(url));
                }
                else
                {
                    client.SendMessage(message.Channel_Id, string.Format("No Images Found For ({0})", string.Join(", ", tags)));
                }
            }
        }
    }
}
using System;

namespace Discord.Objects
{
#pragma warning disable 0649

    public class Message
    {
        public User Author;
        public string Channel_Id;
        public string Content;
        public DateTime? Edited_Timestamp;
        public string Id;
        public bool Mention_Everyone;
        public User[] Mentions;
        public string Nonce;
        public bool Pinned;
        public DateTime Timestamp;
        public bool Tts;
        //mention_roles
        //embeds
        //attachments
    }

    public class User
    {
        public string Avatar;
        public string Discriminator;
        public string Id;
        public string Username;
    }

#pragma warning restore 0649
}
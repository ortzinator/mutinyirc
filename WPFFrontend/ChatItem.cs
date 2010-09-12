namespace WPFFrontend
{
    using System;
    using FlamingIRC;

    public class ChatItem
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }

        public ChatItem() { }

        public ChatItem(DateTime time, string message)
        {
            Time = time;
            Message = message;
        }
    }

    public class ChannelMessageChatItem : ChatItem
    {
        public User User { get; set; }

        public ChannelMessageChatItem() { }

        public ChannelMessageChatItem(DateTime time, string message, User user)
            : base(time, message)
        {
            User = user;
        }
    }
}

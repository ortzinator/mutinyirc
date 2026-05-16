namespace FlamingIRC
{
    public class IrcMessage
    {
        public string Command { get; set; }

        public string From { get; set; }

        public string Message { get; set; }

        public ReplyCode ReplyCode { get; set; }

        public string[] Tokens { get; set; }

        public string Target { get; set; }
    }
}

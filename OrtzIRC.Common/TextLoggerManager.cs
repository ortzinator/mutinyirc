using FlamingIRC;

namespace OrtzIRC.Common
{
    /// <summary>
    ///     Manages the lifecycle of <see cref="TextLogger" />: subscribes to server and channel
    ///     events when logging is enabled, and unsubscribes when disabled.
    /// </summary>
    public static class TextLoggerManager
    {
        private static bool _loggerActive;

        public static bool LoggerActive
        {
            get { return _loggerActive; }

            set
            {
                if (value == _loggerActive) return;

                if (value)
                    TurnOn();
                else
                    TurnOff();

                _loggerActive = value;
            }
        }

        public static bool AddTimestamp
        {
            get { return TextLogger.AddTimestamp; }
            set { TextLogger.AddTimestamp = value; }
        }

        public static string TimeFormat
        {
            get { return TextLogger.TimeFormat; }
            set { TextLogger.TimeFormat = value; }
        }

        public static void TextEntry(Server network, string text)
        {
            if (LoggerActive) TextLogger.TextEntry(network, text);
        }

        public static void TextEntry(Server network, User person, string text)
        {
            if (LoggerActive) TextLogger.TextEntry(network, person, text);
        }

        public static void TextEntry(Channel chan, string text)
        {
            if (LoggerActive) TextLogger.TextEntry(chan, text);
        }

        public static void TurnOn()
        {
            Server.ChannelCreated += ChannelManager_ElementCreated;
            Server.ChannelRemoved += ChannelManager_ElementRemoved;

            ServerManager.Instance.ServerAdded += ServerManager_ElementCreated;
            ServerManager.Instance.ServerRemoved += ServerManager_ElementRemoved;

            foreach (Server ntw in ServerManager.Instance.ServerList)
            {
                TextLogger.AddLoggable(ntw);

                foreach (Channel chan in ntw.Channels.Values)
                    TextLogger.AddLoggable(chan);
            }
        }

        public static void TurnOff()
        {
            Server.ChannelCreated -= ChannelManager_ElementCreated;
            Server.ChannelRemoved -= ChannelManager_ElementRemoved;

            ServerManager.Instance.ServerAdded -= ServerManager_ElementCreated;
            ServerManager.Instance.ServerRemoved -= ServerManager_ElementRemoved;

            TextLogger.RemoveAllLoggables();
        }

        private static void ServerManager_ElementRemoved(object sender, ServerEventArgs args)
        {
            TextLogger.RemoveLoggable(args.Server);
        }

        private static void ServerManager_ElementCreated(object sender, ServerEventArgs args)
        {
            TextLogger.AddLoggable(args.Server);
        }

        private static void ChannelManager_ElementRemoved(object sender, ChannelEventArgs args)
        {
            TextLogger.RemoveLoggable(args.Channel);
        }

        private static void ChannelManager_ElementCreated(object sender, ChannelEventArgs args)
        {
            TextLogger.AddLoggable(args.Channel);
        }
    }
}
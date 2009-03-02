namespace OrtzIRC.Common
{
    using System;
    using OrtzIRC;
    using FlamingIRC;

    /// <summary>
    /// Purpose: Handle the environnement around the manager such as subscribing
    ///          to program events and managing whether the logger is on or off.
    ///          it also listens to the various events in the program in order
    ///          to detect any text input and tell the logger what to do.
    /// </summary>
    public static class TextLoggerManager
    {
        private static bool loggerActive = false;
        public static bool LoggerActive
        {
            get { return loggerActive; }

            set
            {
                if (value == loggerActive) return;

                if (value)
                {
                    TurnOn();
                }
                else
                {
                    TurnOff();
                }

                loggerActive = value;
            }
        }

        public static void TextEntry(Server Network, String Text)
        {
            if (LoggerActive)
            {
                TextLogger.TextEntry(Network, Text);
            }
        }

        public static void TextEntry(Server Network, User Person, String Text)
        {
            if (LoggerActive)
            {
                TextLogger.TextEntry(Network, Person, Text);
            }
        }

        public static void TextEntry(Channel Chan, String Text)
        {
            if (LoggerActive)
            {
                TextLogger.TextEntry(Chan, Text);
            }
        }

        public static void TurnOn()
        {
            ChannelManager.ChannelCreated += ChannelManager_ElementCreated;
            ChannelManager.ChannelRemoved += ChannelManager_ElementRemoved;

            ServerManager.Instance.ServerCreated += ChannelManager_ElementCreated;
            ServerManager.Instance.ServerRemoved += ChannelManager_ElementRemoved;

            foreach(Server Ntw in ServerManager.Instance.ServerList)
            {
                TextLogger.AddLoggable(Ntw);

                foreach(Channel Chan in Ntw.ChanManager.Channels.Values)
                {
                    TextLogger.AddLoggable(Chan);
                }
            }
        }

        public static void TurnOff()
        {
            ChannelManager.ChannelCreated -= ChannelManager_ElementCreated;
            ChannelManager.ChannelRemoved -= ChannelManager_ElementRemoved;

            ServerManager.Instance.ServerCreated -= ChannelManager_ElementCreated;
            ServerManager.Instance.ServerRemoved -= ChannelManager_ElementRemoved;

            TextLogger.RemoveAllLoggables();
        }

        private static void ChannelManager_ElementRemoved(Server Ntw, User Person)
        {
            TextLogger.RemoveLoggable(Ntw, Person);
        }

        private static void ChannelManager_ElementCreated(Server Ntw, User Person)
        {
            TextLogger.AddLoggable(Ntw, Person);
        }

        private static void ChannelManager_ElementRemoved(object sender, ServerEventArgs args)
        {
            TextLogger.RemoveLoggable(args.Server);
        }

        private static void ChannelManager_ElementCreated(object sender, ServerEventArgs args)
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

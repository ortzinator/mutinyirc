using FlamingIRC;

namespace OrtzIRC.Common
{
    using System;

    /// <summary>
    /// Purpose: Handle the environnement around the manager such as subscribing
    ///          to program events and managing whether the logger is on or off.
    ///          it also listens to the various events in the program in order
    ///          to detect any text input and tell the logger what to do.
    /// </summary>
    public static class TextLoggerManager
    {
        private static bool loggerActive;

        public static bool LoggerActive
        {
            get { return loggerActive; }

            set
            {
                if (value == loggerActive) return;

                if (value)
                    TurnOn();
                else
                    TurnOff();

                loggerActive = value;
            }
        }

        public static bool AddTimestamp
        {
            get { return TextLogger.addTimestamp; }
            set { TextLogger.addTimestamp = value; }
        }

        public static string TimeFormat
        {
            get { return TextLogger.timeFormat; }
            set { TextLogger.timeFormat = value; }
        }

        public static void TextEntry(Server network, String text)
        {
            if (LoggerActive)
                TextLogger.TextEntry(network, text);
        }

        public static void TextEntry(Server network, User person, String text)
        {
            if (LoggerActive)
                TextLogger.TextEntry(network, person, text);
        }

        public static void TextEntry(Channel chan, String text)
        {
            if (LoggerActive)
                TextLogger.TextEntry(chan, text);
        }

        public static void TurnOn()
        {
            ChannelManager.ChannelCreated += ChannelManager_ElementCreated;
            ChannelManager.ChannelRemoved += ChannelManager_ElementRemoved;

            ServerManager.Instance.ServerAdded += ServerManager_ElementCreated;
            ServerManager.Instance.ServerRemoved += ServerManager_ElementRemoved;

            foreach (Server ntw in ServerManager.Instance.ServerList)
            {
                TextLogger.AddLoggable(ntw);

                foreach (Channel chan in ntw.ChanManager.Channels.Values)
                    TextLogger.AddLoggable(chan);
            }
        }

        public static void TurnOff()
        {
            ChannelManager.ChannelCreated -= ChannelManager_ElementCreated;
            ChannelManager.ChannelRemoved -= ChannelManager_ElementRemoved;

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
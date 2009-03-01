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
        public static bool LoggerActive
        {
            get
            {
                return TextLogger.Active;
            }

            set
            {
                if (value)
                {
                    TurnOn();
                }
                else
                {
                    TurnOff();
                }
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
            ChannelManager.ChannelCreated += new NewChannelEventHandler(ChannelManager_ElementCreated);
            ChannelManager.ChannelRemoved += new DelChannelEventHandler(ChannelManager_ElementRemoved);

            ServerManager.Instance.ServerCreated += new ServerCreatedEventHandler(ChannelManager_ElementCreated);
            ServerManager.Instance.ServerRemoved += new ServerRemovedEventHandler(ChannelManager_ElementRemoved);
        }

        public static void TurnOff()
        {
            ChannelManager.ChannelCreated -= new NewChannelEventHandler(ChannelManager_ElementCreated);
            ChannelManager.ChannelRemoved -= new DelChannelEventHandler(ChannelManager_ElementRemoved);

            ServerManager.Instance.ServerCreated -= new ServerCreatedEventHandler(ChannelManager_ElementCreated);
            ServerManager.Instance.ServerRemoved -= new ServerRemovedEventHandler(ChannelManager_ElementRemoved);
        }

        private static void ChannelManager_ElementRemoved(Server Ntw, User Person)
        {
            TextLogger.RemoveLoggable(Ntw, Person);
        }

        private static void ChannelManager_ElementCreated(Server Ntw, User Person)
        {
            TextLogger.AddLoggable(Ntw, Person);
        }

        private static void ChannelManager_ElementRemoved(Server Ntw)
        {
            TextLogger.RemoveLoggable(Ntw);
        }

        private static void ChannelManager_ElementCreated(Server Ntw)
        {
            TextLogger.AddLoggable(Ntw);
        }

        private static void ChannelManager_ElementRemoved(Channel Chan)
        {
            TextLogger.RemoveLoggable(Chan);
        }

        private static void ChannelManager_ElementCreated(Channel Chan)
        {
            TextLogger.AddLoggable(Chan);
        }
    }
}
using FlamingIRC;

namespace MutinyIRC.Common;

/// <summary>
/// Manages the lifecycle of <see cref="TextLogger" />: subscribes to server and channel
/// events when logging is enabled, and unsubscribes when disabled.
/// </summary>
public static class TextLoggerManager
{
    private static bool _loggerActive;

    public static bool LoggerActive
    {
        get => _loggerActive;

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
        get => TextLogger.AddTimestamp; set => TextLogger.AddTimestamp = value;
    }

    public static string TimeFormat
    {
        get => TextLogger.TimeFormat; set => TextLogger.TimeFormat = value;
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
        ServerManager.Instance.ServerAdded += ServerManager_ElementCreated;
        ServerManager.Instance.ServerRemoved += ServerManager_ElementRemoved;

        foreach (Server ntw in ServerManager.Instance.ServerList)
        {
            HookChannelEvents(ntw);
            TextLogger.AddLoggable(ntw);

            foreach (Channel chan in ntw.Channels.Values)
                TextLogger.AddLoggable(chan);
        }
    }

    public static void TurnOff()
    {
        ServerManager.Instance.ServerAdded -= ServerManager_ElementCreated;
        ServerManager.Instance.ServerRemoved -= ServerManager_ElementRemoved;

        foreach (Server ntw in ServerManager.Instance.ServerList)
            UnhookChannelEvents(ntw);

        TextLogger.RemoveAllLoggables();
    }

    /// <summary>
    /// Starts logging the channels of <paramref name="server" />. Channel events are per-server,
    /// so every server logging is active for needs its own subscription.
    /// </summary>
    private static void HookChannelEvents(Server server)
    {
        server.ChannelCreated += ChannelManager_ElementCreated;
        server.ChannelRemoved += ChannelManager_ElementRemoved;
    }

    /// <summary>
    /// The counterpart to <see cref="HookChannelEvents" />. Detaching matters: the server's log
    /// files are gone by the time a removed server is unhooked, so a channel created afterwards
    /// would look up a table entry that no longer exists.
    /// </summary>
    private static void UnhookChannelEvents(Server server)
    {
        server.ChannelCreated -= ChannelManager_ElementCreated;
        server.ChannelRemoved -= ChannelManager_ElementRemoved;
    }

    private static void ServerManager_ElementRemoved(object sender, ServerEventArgs args)
    {
        UnhookChannelEvents(args.Server);
        TextLogger.RemoveLoggable(args.Server);
    }

    private static void ServerManager_ElementCreated(object sender, ServerEventArgs args)
    {
        HookChannelEvents(args.Server);
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

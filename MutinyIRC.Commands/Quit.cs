namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    ///   Disconnects from the current server (the /quit command). With a message,
    ///   <c>/quit &lt;message&gt;</c> sends it as the quit reason; bare <c>/quit</c> uses a
    ///   random "quit" message, falling back to a default if none are configured.
    /// </summary>
    /// <remarks>
    ///   Works from any window; the command is always routed to that window's server. The tail
    ///   is taken raw so the quit message keeps its spacing.
    /// </remarks>
    [Plugin]
    public class Quit : ICommand
    {
        private const string fallbackMessage = "MutinyIRC";

        private static string DefaultMessage() => RandomMessages.Instance.GetMessage("quit") ?? fallbackMessage;

        /// <summary>Quits with a message from the server window.</summary>
        [RawArguments]
        public void Execute(Server server, string message) => Disconnect(server, message);

        /// <summary>Quits with a message from a channel window.</summary>
        [RawArguments]
        public void Execute(Channel channel, string message) => Disconnect(channel.Server, message);

        /// <summary>Quits with a message from a private message window.</summary>
        [RawArguments]
        public void Execute(PrivateMessageSession pm, string message) => Disconnect(pm.Server, message);

        /// <summary>Quits with the default message from the server window.</summary>
        public void Execute(Server server) => Disconnect(server, DefaultMessage());

        /// <summary>Quits with the default message from a channel window.</summary>
        public void Execute(Channel channel) => Disconnect(channel.Server, DefaultMessage());

        /// <summary>Quits with the default message from a private message window.</summary>
        public void Execute(PrivateMessageSession pm) => Disconnect(pm.Server, DefaultMessage());

        private static void Disconnect(Server server, string message)
        {
            // A whitespace-only tail is still a single token, so it reaches the message overload
            // rather than the parameterless one — treat it as a request for the default.
            if (string.IsNullOrWhiteSpace(message))
                message = DefaultMessage();

            server.Disconnect(message);
        }
    }
}
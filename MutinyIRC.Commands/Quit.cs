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
    [Plugin("Quit", "Disconnects from the current server. Usage: /quit [message]")]
    public class Quit : ICommand
    {
        private const string fallbackMessage = "MutinyIRC";

        private static string DefaultMessage() => RandomMessages.Instance.GetMessage("quit") ?? fallbackMessage;

        /// <summary>Quits with a message from any window.</summary>
        [RawArguments]
        public void Execute(MessageContext context, string message)
            => Disconnect(context.OwningServer, message);

        /// <summary>Quits with the default message from any window.</summary>
        public void Execute(MessageContext context) => Disconnect(context.OwningServer, DefaultMessage());

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
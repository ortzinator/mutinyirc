namespace OrtzIRC.Commands
{
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;

    /// <summary>
    /// Creates new connections to IRC servers
    /// </summary>
    [Plugin("Server")]
    public class ServerCommand : ICommand
    {
        /// <summary>
        /// Summary of command with these specific parameters
        /// </summary>
        public void Execute(Server context, char[] switches, string server)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(switches);
            System.Diagnostics.Debug.WriteLine("Server command with switches: " + sb + "to server: " + server);
        }

        /// <summary>
        /// Summary of command with these specific parameters
        /// </summary>
        public void Execute(Server context, string server)
        {
            context.ChangeServer(new ServerSettings { Url = server });
            context.Connect();
        }
    }
}

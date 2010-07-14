namespace OrtzIRC.Commands
{
    using System;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;
    using OrtzIRC.Properties;

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
            foreach (char c in switches)
            {
                switch (c)
                {
                    case 'n': //New window and connect
                        var svr = ServerManager.Instance.Create(Settings.Default.FirstNick, server, false);
                        svr.Connect();
                        return;
                }
            }
        }

        /// <summary>
        /// Connect to a server in the same window
        /// </summary>
        public void Execute(Server context, string server)
        {
            context.ChangeServer(Settings.Default.FirstNick, server, false);
            context.Connect();
        }
    }
}

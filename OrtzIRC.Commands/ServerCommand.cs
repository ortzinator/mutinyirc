namespace OrtzIRC.Commands
{
    using System;
    using System.Threading;
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
            foreach (var c in switches)
            {
                switch (c)
                {
                    case 'm': //New window and connect
                        var svr = ServerManager.Instance.Create(Settings.Default.FirstNick, server, false);
                        svr.Connect();
                        return;
                    case 'n': //New window, don't connect
                        ServerManager.Instance.Create(Settings.Default.FirstNick, server, false);
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
            var th = new Thread((ThreadStart)delegate
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                context.Connect();
            });
            th.Start();
        }
    }
}

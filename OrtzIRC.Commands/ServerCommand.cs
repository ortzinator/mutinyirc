using FlamingIRC;

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
            Execute(context, switches, server, "6667");
        }

        public void Execute(Server context, char[] switches, string server, string port)
        {
            foreach (char c in switches)
            {
                switch (c)
                {
                    case 'n': //New window and connect
                        var args = new ConnectionArgs(Settings.Default.FirstNick, server, false);
                        try
                        {
                            args.Port = int.Parse(port);
                        }
                        catch (Exception)
                        {
                            args.Port = 6667;
                        }
                        var svr = ServerManager.Instance.Create(args);
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

        public void Execute(Server context, string server, string port)
        {
            var args = new ConnectionArgs(Settings.Default.FirstNick, server, false);
            try
            {
                args.Port = int.Parse(port);
            }
            catch (Exception)
            {
                args.Port = 6667;
            }

            context.ChangeServer(args);
            context.Connect();
        }
    }
}

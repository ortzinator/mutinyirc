using System;
using System.Threading;

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
            foreach (var c in switches)
            {
                switch (c)
                {
                    case 'm':
                        var svr = ServerManager.Instance.Create(new ServerSettings { Url = server, Nick = OrtzIRC.Properties.Settings.Default.FirstNick });
                        svr.Connect();
                        return;
                    case 'n':
                        ServerManager.Instance.Create(new ServerSettings { Url = server, Nick = OrtzIRC.Properties.Settings.Default.FirstNick });
                        return;
                }
            }
        }

        /// <summary>
        /// Summary of command with these specific parameters
        /// </summary>
        public void Execute(Server context, string server)
        {
            context.ChangeServer(new ServerSettings { Url = server, Nick = OrtzIRC.Properties.Settings.Default.FirstNick });
            var th = new Thread((ThreadStart)delegate
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                context.Connect();
            });
            th.Start();
        }
    }
}

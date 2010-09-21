namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using FlamingIRC;
    using OrtzIRC.Common;
    using System.Windows.Threading;

    public class ServerViewModel : IrcViewModel
    {
        public Server Server { get; private set; }

        public ServerViewModel(Server server)
        {
            Server = server;
            Name = Server.Url;
            Server.RawMessageReceived += Server_RawMessageReceived;
        }

        private void Server_RawMessageReceived(object sender, DataEventArgs<string> e)
        {
            ChatLines.Add(new ChatItemViewModel(DateTime.Now, e.Data));
        }
    }
}

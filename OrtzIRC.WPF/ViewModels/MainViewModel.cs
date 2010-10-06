namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using FlamingIRC;
    using MvvmFoundation.Wpf;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;
    using OrtzIRC.WPF.Properties;

    public class MainViewModel : ViewModelBase
    {
        public MTObservableCollection<IrcViewModel> Panels { get; protected set; }

        public MainViewModel()
        {
            Panels = new MTObservableCollection<IrcViewModel>();

            System.Windows.DependencyObject dep = new System.Windows.DependencyObject();
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(dep))
            {
                //Executes in Design mode. Use for mockups.
                Panels.Add(new ServerViewModel());
                return;
            }

            Settings.Default.SettingsSaving += Default_SettingsSaving;
            ServerManager.Instance.ServerAdded += Instance_ServerCreated;

            LoadSettings();

            foreach (ServerSettings server in IrcSettingsManager.Instance.GetAutoConnectServers())
            {
                if (server.Nick == null)
                    server.Nick = Settings.Default.FirstNick;

                Server newServer = ServerManager.Instance.Create(new ConnectionArgs(server.Nick, server.Url, server.Ssl));
                newServer.JoinSelf += Server_JoinSelf;
                newServer.Connect();
            }

            //PluginManager.LoadPlugins(Path.Combine(Environment.CurrentDirectory, "plugins"));
            //PluginManager.LoadPlugins(Settings.Default.UserPluginDirectory);
            RandomMessages.Load();
        }

        private void Server_JoinSelf(object sender, DataEventArgs<Channel> e)
        {
            var chan = new ChannelViewModel(e.Data);
            chan.RequestClose += Chan_RequestClose;
            Panels.Add(chan);
        }

        private void Chan_RequestClose(object sender, EventArgs e)
        {
            var chan = (ChannelViewModel)sender;
            chan.RequestClose -= Chan_RequestClose;
            Panels.Remove(chan);
        }

        private void Default_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            TextLoggerManager.LoggerActive = Settings.Default.LoggerActivated;
            TextLoggerManager.AddTimestamp = Settings.Default.LoggerTimestampsActivated;
            TextLoggerManager.TimeFormat = Settings.Default.LoggerTimestampFormat;
        }

        private void Instance_ServerCreated(object sender, ServerEventArgs e)
        {
            CreateServerPanel(e.Server);
            //e.Server.PrivateMessageSessionAdded += Server_PrivateMessageSessionAdded;
        }

        private void CreateServerPanel(Server server)
        {
            Panels.Add(new ServerViewModel(server));
        }

        public override void Close()
        {
            for (int i = 0; i < Panels.Count; i++)
            {
                IrcViewModel viewModel = Panels[i];
                viewModel.Close();
            }

            base.Close();
        }
    }
}

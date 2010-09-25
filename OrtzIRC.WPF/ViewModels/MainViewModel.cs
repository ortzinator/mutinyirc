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

    public class MainViewModel : ObservableObject
    {
        public MTObservableCollection<IrcViewModel> Panels { get; protected set; }

        public MainViewModel()
        {
            Panels = new MTObservableCollection<IrcViewModel>();

            System.Windows.DependencyObject dep = new System.Windows.DependencyObject();
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(dep))
            {
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
            Panels.Add(new ChannelViewModel(e.Data));
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
    }
}

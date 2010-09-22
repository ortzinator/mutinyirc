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
        public ObservableCollection<IrcViewModel> Panels { get; private set; }

        public MainViewModel()
        {
            Panels = new ObservableCollection<IrcViewModel>();
            
            System.Windows.DependencyObject dep = new System.Windows.DependencyObject();
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(dep))
                return;

            Settings.Default.SettingsSaving += Default_SettingsSaving;
            ServerManager.Instance.ServerAdded += Instance_ServerCreated;

            LoadSettings();

            foreach (ServerSettings server in IrcSettingsManager.Instance.GetAutoConnectServers())
            {
                if (server.Nick == null)
                    server.Nick = Settings.Default.FirstNick;

                Server newServer = ServerManager.Instance.Create(new ConnectionArgs(server.Nick, server.Url, server.Ssl));
                newServer.Connect();
            }

            //PluginManager.LoadPlugins(Path.Combine(Environment.CurrentDirectory, "plugins"));
            //PluginManager.LoadPlugins(Settings.Default.UserPluginDirectory);
            RandomMessages.Load();
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

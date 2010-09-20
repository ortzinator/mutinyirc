using System;
using System.Windows.Input;
using FlamingIRC;
using OrtzIRC.Common;
using OrtzIRC.WPF.Properties;

namespace OrtzIRC.WPF
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChannelPanel_CommandEntered(object sender, CommandEventArgs e)
        {
            ((ChatCollection)Resources["chatCollection"]).Add(new ChannelMessageChatItem { Time = DateTime.Now, User = new User { Nick = "Ortzinator" }, Message = e.Data });
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.T)
            {
                ((ChatCollection)Resources["chatCollection"]).Add(new ChatItem(DateTime.Now, "Some message."));
            }
            base.OnKeyDown(e);
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            Settings.Default.SettingsSaving += Settings_SettingsSaving;
            ServerManager.Instance.ServerAdded += ServerManager_ServerAdded;

            LoadSettings();

            if (MessageBox.Show("Do you wish to connect?", "Debug", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) //hack
            {
                foreach (ServerSettings server in IrcSettingsManager.Instance.GetAutoConnectServers())
                {
                    if (server.Nick == null)
                        server.Nick = Settings.Default.FirstNick;

                    Server newServer = ServerManager.Instance.Create(new ConnectionArgs(server.Nick, server.Url, server.Ssl));
                    newServer.Connect();
                }
            }
        }

        private void ServerManager_ServerAdded(object sender, ServerEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Settings_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void LoadSettings()
        {
            TextLoggerManager.LoggerActive = Settings.Default.LoggerActivated;
            TextLoggerManager.AddTimestamp = Settings.Default.LoggerTimestampsActivated;
            TextLoggerManager.TimeFormat = Settings.Default.LoggerTimestampFormat;
        }
    }
}

using System;
using System.Windows.Input;
using FlamingIRC;
using OrtzIRC.Common;
using WPFFrontend.Properties;

namespace WPFFrontend
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
            Settings.Default.SettingsSaving += Default_SettingsSaving;
            ServerManager.Instance.ServerAdded += Instance_ServerAdded;
        }

        void Instance_ServerAdded(object sender, ServerEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Default_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

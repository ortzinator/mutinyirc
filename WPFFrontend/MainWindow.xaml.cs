using System;
using System.Windows.Input;
using FlamingIRC;

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
                ((ChatCollection)Resources["chatCollection"]).Add(new ChatItem{});
            }
            base.OnKeyDown(e);
        }
    }
}

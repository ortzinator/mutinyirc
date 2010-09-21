namespace OrtzIRC.WPF
{
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ServerView.xaml
    /// </summary>
    public partial class ServerView : UserControl
    {
        public ServerView()
        {
            InitializeComponent();
        }

        private void commandBox_CommandEntered(object sender, CommandEventArgs e)
        {
            outputBox.scrollViewer.ScrollToBottom();
        }
    }
}

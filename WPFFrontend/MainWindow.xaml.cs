using System;

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
            ((ChatCollection)Resources["chatCollection"]).Add(new ChatRowItem(){Time = DateTime.Now, User = "Ortzinator", Message = e.Data});
        }
    }
}

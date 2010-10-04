namespace OrtzIRC.WPF
{
    using System.Windows.Controls;
    using System.Windows.Data;
using System.ComponentModel;

    /// <summary>
    /// Interaction logic for ChannelPanel.xaml
    /// </summary>
    public partial class ChannelView : UserControl
    {
        public ChannelView()
        {
            InitializeComponent();

            var dv = CollectionViewSource.GetDefaultView(userBox);

            //dv.
            //userBox.Items.sort
        }
    }
}

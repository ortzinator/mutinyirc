namespace WPFFrontend
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for ChannelPanel.xaml
    /// </summary>
    public partial class ChannelPanel : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            IrcOutputBox.ItemsSourceProperty.AddOwner(typeof(ChannelPanel));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        //public static readonly RoutedEvent CommandEnteredEvent =
        //    CommandTextBox.CommandEnteredEvent.AddOwner(typeof(ChannelPanel));

        public ChannelPanel()
        {
            InitializeComponent();
        }
    }
}

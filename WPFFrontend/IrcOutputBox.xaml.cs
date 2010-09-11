namespace WPFFrontend
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.ComponentModel;
    using System.Collections;

    /// <summary>
    /// Interaction logic for IrcOutputBox.xaml
    /// </summary>
    public partial class IrcOutputBox : UserControl
    {
        public IrcOutputBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            ItemsControl.ItemsSourceProperty.AddOwner(typeof(IrcOutputBox));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
    }
}

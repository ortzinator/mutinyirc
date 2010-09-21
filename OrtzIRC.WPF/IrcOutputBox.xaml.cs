namespace OrtzIRC.WPF
{
    using System;
    using System.Collections;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for IrcOutputBox.xaml
    /// </summary>
    public partial class IrcOutputBox : UserControl
    {
        public IrcOutputBox()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 2);
            timer.Tick += ((sender, e) =>
            {
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                {
                    scrollViewer.ScrollToEnd();
                }
            });
            timer.Start();

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

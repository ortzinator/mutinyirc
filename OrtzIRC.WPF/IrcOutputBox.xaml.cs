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
        public static readonly DependencyProperty ScrolledProperty =
            DependencyProperty.Register("Scrolled", typeof(bool), typeof(IrcOutputBox));

        public bool Scrolled
        {
            get { return (bool)GetValue(ScrolledProperty); }
            set { SetValue(ScrolledProperty, value);}
        }

        public IrcOutputBox()
        {
            InitializeComponent();

            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 2);
            timer.Tick += ((sender, e) =>
            {
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                {
                    scrollViewer.ScrollToEnd();
                    Scrolled = false;
                }
                else
                {
                    Scrolled = true;
                }
            });
            timer.Start();
        }
    }
}

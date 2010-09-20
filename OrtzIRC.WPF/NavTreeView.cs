namespace OrtzIRC.WPF
{
    using System.Windows;
    using System.Windows.Controls;

    public class NavTreeView : ListBox
    {
        static NavTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavTreeView), new FrameworkPropertyMetadata(typeof(NavTreeView)));
        }
    }
}

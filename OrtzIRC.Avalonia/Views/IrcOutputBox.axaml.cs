namespace OrtzIRC.Avalonia.Views;

using System;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Threading;

public partial class IrcOutputBox : UserControl
{
    public static readonly DirectProperty<IrcOutputBox, bool> ScrolledProperty =
        AvaloniaProperty.RegisterDirect<IrcOutputBox, bool>(
            nameof(Scrolled),
            o => o.Scrolled,
            (o, v) => o.Scrolled = v);

    private bool _scrolled;
    public bool Scrolled
    {
        get => _scrolled;
        set => SetAndRaise(ScrolledProperty, ref _scrolled, value);
    }

    public IrcOutputBox()
    {
        InitializeComponent();

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        timer.Tick += (sender, e) =>
        {
            if (scrollViewer.Offset.Y >= scrollViewer.Extent.Height - scrollViewer.Viewport.Height)
            {
                scrollViewer.ScrollToEnd();
                Scrolled = false;
            }
            else
            {
                Scrolled = true;
            }
        };
        timer.Start();
    }
}

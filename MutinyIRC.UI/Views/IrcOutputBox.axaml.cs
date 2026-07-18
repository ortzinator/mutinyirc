using System.Collections.Specialized;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Threading;

namespace MutinyIRC.UI.Views;

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

    private bool _isPinned = true;
    private INotifyCollectionChanged? _subscribedCollection;

    public IrcOutputBox()
    {
        InitializeComponent();

        // Track pin state whenever the scroll offset changes.
        scrollViewer.PropertyChanged += (s, e) =>
        {
            if (e.Property != ScrollViewer.OffsetProperty) return;

            var scrollableHeight = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;
            var atBottom = scrollableHeight <= 0 || scrollViewer.Offset.Y >= scrollableHeight - 1;
            _isPinned = atBottom;
            Scrolled = !atBottom;
        };

        // Re-wire the collection listener whenever the ItemsSource binding resolves or changes.
        outputItems.PropertyChanged += (s, e) =>
        {
            if (e.Property != ItemsControl.ItemsSourceProperty) return;

            if (_subscribedCollection != null)
                _subscribedCollection.CollectionChanged -= OnItemsChanged;

            _subscribedCollection = outputItems.ItemsSource as INotifyCollectionChanged;

            if (_subscribedCollection != null)
                _subscribedCollection.CollectionChanged += OnItemsChanged;
        };
    }

    private void ScrollIndicator_PointerPressed(object sender, PointerPressedEventArgs e) => ScrollToBottom();

    public void ScrollToBottom()
    {
        _isPinned = true;
        Dispatcher.UIThread.Post(() => scrollViewer.ScrollToEnd(), DispatcherPriority.Loaded);
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && _isPinned)
        {
            // Defer until after the new item has been measured and arranged.
            Dispatcher.UIThread.Post(() => scrollViewer.ScrollToEnd(), DispatcherPriority.Loaded);
        }
    }
}

namespace MutinyIRC.UI.Views;

using System;
using System.Linq;
using System.Threading.Tasks;
using global::Avalonia;
using global::Avalonia.Animation;
using global::Avalonia.Animation.Easings;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.Layout;
using global::Avalonia.Styling;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using UI.Controls;

public partial class ChannelView : UserControl
{
    private static readonly TimeSpan TopicSlideDuration = TimeSpan.FromMilliseconds(160);

    // Captured when the topic expands so collapse can slide back to the same single-line height
    // (the collapsed TextBlock is hidden while expanded, so its DesiredSize is unavailable then).
    private double _topicCollapsedHeight;
    public ChannelView()
    {
        InitializeComponent();
        commandBox.CommandEntered += (_, _) => outputBox.ScrollToBottom();
        sendButton.Click += (_, _) =>
        {
            commandBox.Submit();
            commandBox.Focus();
        };

        // Right-click doesn't select a ListBox row by default, but the user-list context menu
        // targets the selected user — so select the row under the cursor before the menu opens.
        userBox.AddHandler(PointerPressedEvent, UserBox_PointerPressed, RoutingStrategies.Tunnel);

        // Topic strip: click to expand the full selectable topic, collapse when focus leaves.
        topicStrip.AddHandler(PointerPressedEvent, TopicStrip_PointerPressed, RoutingStrategies.Tunnel);
        topicStrip.LostFocus += TopicStrip_LostFocus;
    }

    private async void TopicStrip_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (topicExpanded.IsVisible)
            return;

        var collapsedHeight = topicStrip.Bounds.Height;
        _topicCollapsedHeight = collapsedHeight;
        topicCollapsed.IsVisible = false;
        topicExpanded.IsVisible = true;

        // Focus the strip so the LostFocus handler can detect when focus moves away.
        topicStrip.Focus();

        // Also watch for clicks anywhere outside the strip — a click on a non-focusable element
        // won't move focus, so LostFocus alone wouldn't catch it.
        TopLevel.GetTopLevel(this)?.AddHandler(
            PointerPressedEvent, TopLevel_PointerPressed, RoutingStrategies.Tunnel);

        // Measure the full topic at the current width *before* pinning the height — otherwise the
        // pinned value would be reported back as the desired size and nothing would animate.
        topicStrip.Measure(new Size(topicStrip.Bounds.Width, double.PositiveInfinity));
        var targetHeight = topicStrip.DesiredSize.Height;

        // Pin to the collapsed height so the strip doesn't jump before the slide, then expand.
        topicStrip.Height = collapsedHeight;
        await SlideTopicHeightAsync(collapsedHeight, targetHeight);

        // Hand height back to auto so the strip reflows when the window is resized.
        if (topicExpanded.IsVisible)
            topicStrip.Height = double.NaN;
    }

    private void TopicStrip_LostFocus(object? sender, RoutedEventArgs e)
    {
        // LostFocus fires while focus is still moving; defer the check so the new focus target
        // is settled, then collapse only if focus landed outside the topic strip.
        Dispatcher.UIThread.Post(() =>
        {
            var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Visual;
            var stillInside = focused != null
                && (focused == topicStrip || focused.GetVisualAncestors().Contains(topicStrip));
            if (!stillInside)
                CollapseTopic();
        });
    }

    private void TopLevel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var clicked = e.Source as Visual;
        var insideStrip = clicked != null
            && (clicked == topicStrip || clicked.GetVisualAncestors().Contains(topicStrip));
        if (!insideStrip)
            CollapseTopic();
    }

    private async void CollapseTopic()
    {
        if (!topicExpanded.IsVisible)
            return;

        TopLevel.GetTopLevel(this)?.RemoveHandler(PointerPressedEvent, TopLevel_PointerPressed);

        // Slide the strip back up to the single-line height, then swap back to the collapsed view.
        await SlideTopicHeightAsync(topicStrip.Bounds.Height, _topicCollapsedHeight);

        topicExpanded.IsVisible = false;
        topicCollapsed.IsVisible = true;
        topicStrip.Height = double.NaN;
    }

    private Task SlideTopicHeightAsync(double from, double to)
    {
        var animation = new Animation
        {
            Duration = TopicSlideDuration,
            Easing = new CubicEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(Layoutable.HeightProperty, from) },
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(Layoutable.HeightProperty, to) },
                },
            },
        };
        return animation.RunAsync(topicStrip);
    }

    private void UserBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(userBox).Properties.IsRightButtonPressed)
            return;

        var item = (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true);
        if (item != null)
            userBox.SelectedItem = item.DataContext;
    }
}

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

    private enum TopicState { Collapsed, Expanding, Expanded, Collapsing }

    // Single source of truth for the topic's lifecycle. The expand/collapse handlers gate on this
    // instead of inferring state from IsVisible, which is only mutated *after* the slide awaits and
    // so can't safely guard against concurrent triggers (a click outside fires both the outside-click
    // and LostFocus paths).
    private TopicState _topicState = TopicState.Collapsed;

    // Captured when the topic expands so collapse can slide back to the same single-line height
    // (the collapsed TextBlock is hidden while expanded, so its DesiredSize is unavailable then).
    private double _topicCollapsedHeight;

    // The TopLevel we attached the outside-click handler to, held so it can be removed on collapse
    // or detach even when GetTopLevel(this) would return null mid-teardown.
    private TopLevel? _topicOutsideClickRoot;

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
        if (_topicState != TopicState.Collapsed)
            return;
        _topicState = TopicState.Expanding;

        var collapsedHeight = topicStrip.Bounds.Height;
        _topicCollapsedHeight = collapsedHeight;
        topicCollapsed.IsVisible = false;
        topicExpanded.IsVisible = true;

        // Focus the strip so the LostFocus handler can detect when focus moves away.
        topicStrip.Focus();

        // Also watch for clicks anywhere outside the strip — a click on a non-focusable element
        // won't move focus, so LostFocus alone wouldn't catch it.
        AddOutsideClickHandler();

        // Measure the full topic at the current width *before* pinning the height — otherwise the
        // pinned value would be reported back as the desired size and nothing would animate.
        topicStrip.Measure(new Size(topicStrip.Bounds.Width, double.PositiveInfinity));
        var targetHeight = topicStrip.DesiredSize.Height;

        // Pin to the collapsed height so the strip doesn't jump before the slide, then expand.
        topicStrip.Height = collapsedHeight;
        await SlideTopicHeightAsync(collapsedHeight, targetHeight);

        // Hand height back to auto so the strip reflows when the window is resized.
        topicStrip.Height = double.NaN;
        _topicState = TopicState.Expanded;
    }

    private void TopicStrip_LostFocus(object? sender, RoutedEventArgs e)
    {
        // LostFocus fires while focus is still moving; defer the check so the new focus target
        // is settled, then collapse only if focus landed outside the topic strip.
        Dispatcher.UIThread.Post(() =>
        {
            var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Visual;
            if (!IsInsideTopic(focused))
                CollapseTopic();
        });
    }

    private void TopLevel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsInsideTopic(e.Source as Visual))
            CollapseTopic();
    }

    // True when the visual is the topic strip or lives inside it — used by both collapse triggers
    // to ignore focus moves and clicks that stay within the expanded topic.
    private bool IsInsideTopic(Visual? v) =>
        v != null && (v == topicStrip || v.GetVisualAncestors().Contains(topicStrip));

    private async void CollapseTopic()
    {
        if (_topicState != TopicState.Expanded)
            return;
        _topicState = TopicState.Collapsing;

        RemoveOutsideClickHandler();

        // Slide the strip back up to the single-line height, then swap back to the collapsed view.
        await SlideTopicHeightAsync(topicStrip.Bounds.Height, _topicCollapsedHeight);

        topicExpanded.IsVisible = false;
        topicCollapsed.IsVisible = true;
        topicStrip.Height = double.NaN;
        _topicState = TopicState.Collapsed;
    }

    private void AddOutsideClickHandler()
    {
        _topicOutsideClickRoot = TopLevel.GetTopLevel(this);
        _topicOutsideClickRoot?.AddHandler(
            PointerPressedEvent, TopLevel_PointerPressed, RoutingStrategies.Tunnel);
    }

    private void RemoveOutsideClickHandler()
    {
        _topicOutsideClickRoot?.RemoveHandler(PointerPressedEvent, TopLevel_PointerPressed);
        _topicOutsideClickRoot = null;
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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // The outside-click handler lives on the TopLevel, not on this view, so it would otherwise
        // outlive a ChannelView that's removed while its topic is expanded.
        RemoveOutsideClickHandler();
        base.OnDetachedFromVisualTree(e);
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

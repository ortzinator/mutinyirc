namespace MutinyIRC.UI.Controls;

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

/// <summary>
///   A channel topic strip that shows a single trimmed line and expands to the full, selectable
///   text on click, sliding back to one line when focus or a click moves elsewhere. Hides itself
///   while the topic is empty.
/// </summary>
public partial class TopicBar : UserControl
{
    public static readonly StyledProperty<string?> TopicProperty =
        AvaloniaProperty.Register<TopicBar, string?>(nameof(Topic));

    public string? Topic
    {
        get => GetValue(TopicProperty);
        set => SetValue(TopicProperty, value);
    }

    private static readonly TimeSpan SlideDuration = TimeSpan.FromMilliseconds(160);

    private enum State { Collapsed, Expanding, Expanded, Collapsing }

    // Single source of truth for the lifecycle. The expand/collapse handlers gate on this instead
    // of inferring state from IsVisible, which is only mutated *after* the slide awaits and so can't
    // safely guard against concurrent triggers (a click outside fires both the outside-click and
    // LostFocus paths).
    private State _state = State.Collapsed;

    // Captured when the topic expands so collapse can slide back to the same single-line height
    // (the collapsed TextBlock is hidden while expanded, so its DesiredSize is unavailable then).
    private double _collapsedHeight;

    // The TopLevel we attached the outside-click handler to, held so it can be removed on collapse
    // or detach even when GetTopLevel(this) would return null mid-teardown.
    private TopLevel? _outsideClickRoot;

    public TopicBar()
    {
        InitializeComponent();

        AddHandler(PointerPressedEvent, OnPressed, RoutingStrategies.Tunnel);
        LostFocus += OnLostFocus;
    }

    private async void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_state != State.Collapsed)
            return;
        _state = State.Expanding;

        _collapsedHeight = Bounds.Height;
        topicCollapsed.IsVisible = false;
        topicExpanded.IsVisible = true;

        // Focus the strip so the LostFocus handler can detect when focus moves away.
        Focus();

        // Also watch for clicks anywhere outside the strip — a click on a non-focusable element
        // won't move focus, so LostFocus alone wouldn't catch it.
        AddOutsideClickHandler();

        // Measure the full topic at the current width *before* pinning the height — otherwise the
        // pinned value would be reported back as the desired size and nothing would animate.
        Measure(new Size(Bounds.Width, double.PositiveInfinity));
        var targetHeight = DesiredSize.Height;

        // Pin to the collapsed height so the strip doesn't jump before the slide, then expand.
        Height = _collapsedHeight;
        await SlideHeightAsync(_collapsedHeight, targetHeight);

        // Hand height back to auto so the strip reflows when the window is resized.
        Height = double.NaN;
        _state = State.Expanded;
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        // LostFocus fires while focus is still moving; defer the check so the new focus target is
        // settled, then collapse only if focus landed outside the strip.
        Dispatcher.UIThread.Post(() =>
        {
            var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Visual;
            if (!IsInside(focused))
                Collapse();
        });
    }

    private void OnOutsidePressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsInside(e.Source as Visual))
            Collapse();
    }

    // True when the visual is the strip or lives inside it — used by both collapse triggers to
    // ignore focus moves and clicks that stay within the expanded topic.
    private bool IsInside(Visual? v) =>
        v != null && (v == this || v.GetVisualAncestors().Contains(this));

    private async void Collapse()
    {
        if (_state != State.Expanded)
            return;
        _state = State.Collapsing;

        RemoveOutsideClickHandler();

        // Slide back up to the single-line height, then swap back to the collapsed view.
        await SlideHeightAsync(Bounds.Height, _collapsedHeight);

        topicExpanded.IsVisible = false;
        topicCollapsed.IsVisible = true;
        Height = double.NaN;
        _state = State.Collapsed;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // The outside-click handler lives on the TopLevel, not on this control, so it would
        // otherwise outlive a TopicBar that's removed while its topic is expanded.
        RemoveOutsideClickHandler();
        base.OnDetachedFromVisualTree(e);
    }

    private void AddOutsideClickHandler()
    {
        _outsideClickRoot = TopLevel.GetTopLevel(this);
        _outsideClickRoot?.AddHandler(
            PointerPressedEvent, OnOutsidePressed, RoutingStrategies.Tunnel);
    }

    private void RemoveOutsideClickHandler()
    {
        _outsideClickRoot?.RemoveHandler(PointerPressedEvent, OnOutsidePressed);
        _outsideClickRoot = null;
    }

    private Task SlideHeightAsync(double from, double to)
    {
        var animation = new Animation
        {
            Duration = SlideDuration,
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
        return animation.RunAsync(this);
    }
}

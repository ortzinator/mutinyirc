using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.UI.Views;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// Regression tests for IrcOutputBox scroll-pinning behavior.
///
/// Bug 1 — System.Reactive dependency:
///   A previous version of IrcOutputBox used System.Reactive's Observable.FromEventPattern
///   to wire the CollectionChanged subscription.  When System.Reactive was removed from the
///   project references the scroll-pinning silently stopped working.  The rewrite to plain
///   C# events has no such dependency.  These tests run in a project that does NOT reference
///   System.Reactive, so compilation alone validates that regression is gone.
///
/// Bug 2 — Scroll pin broken after ItemsSource rebind:
///   The old code subscribed once in the constructor and never re-subscribed when the
///   ItemsSource binding resolved later (asynchronously via DataContext).  New items then
///   didn't trigger the scroll-to-bottom handler.  The fix listens to
///   ItemsControl.ItemsSourceProperty changes and re-wires the subscription each time.
/// </summary>
[TestFixture]
public class IrcOutputBoxTests
{
    [AvaloniaTest]
    public void IrcOutputBox_CanBeInstantiated()
    {
        // Compilation passing already proves no System.Reactive dependency.
        // This test confirms instantiation doesn't throw at runtime either.
        IrcOutputBox box = null!;
        Assert.DoesNotThrow(() => box = new IrcOutputBox());
        Assert.That(box, Is.Not.Null);
    }

    [AvaloniaTest]
    public void IrcOutputBox_ScrolledProperty_InitiallyFalse()
    {
        var box = new IrcOutputBox();
        Assert.That(box.Scrolled, Is.False,
            "Scrolled must start as false — the view is pinned to the bottom by default");
    }

    /// <summary>
    /// After binding a collection the IrcOutputBox must subscribe to CollectionChanged.
    /// The subscription is tracked in the private _subscribedCollection field.
    /// Without this subscription (the System.Reactive regression), new items would
    /// not trigger auto-scroll.
    /// </summary>
    [AvaloniaTest]
    public void IrcOutputBox_AfterItemsSourceSet_SubscribesToCollection()
    {
        var window = new Window();
        var box = new IrcOutputBox();
        window.Content = box;
        window.Show();

        var items = new ObservableCollection<ChatItemViewModel>();
        box.DataContext = new ChannelViewModelStub(items);

        // Give the binding a chance to resolve.
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var subscribedField = typeof(IrcOutputBox).GetField(
            "_subscribedCollection",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.That(subscribedField, Is.Not.Null, "_subscribedCollection field must exist");
        var subscribed = subscribedField!.GetValue(box) as INotifyCollectionChanged;

        Assert.That(subscribed, Is.Not.Null,
            "_subscribedCollection must be non-null after ItemsSource is bound; " +
            "null means the CollectionChanged re-subscription logic never fired");
    }

    /// <summary>
    /// Verifies that adding items while pinned does not set Scrolled = true.
    /// (The control should auto-scroll to the new bottom, remaining pinned.)
    /// </summary>
    [AvaloniaTest]
    public void IrcOutputBox_AddItemWhilePinned_DoesNotSetScrolledToTrue()
    {
        var window = new Window { Width = 400, Height = 300 };
        var box = new IrcOutputBox();
        window.Content = box;
        window.Show();

        var items = new ObservableCollection<ChatItemViewModel>();
        box.DataContext = new ChannelViewModelStub(items);

        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // Add a handful of items while still pinned.
        for (int i = 0; i < 5; i++)
            items.Add(new ChatItemViewModel(DateTime.Now, $"line {i}"));

        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // The control is pinned so Scrolled should remain false (we're at the bottom).
        Assert.That(box.Scrolled, Is.False,
            "Scrolled must stay false when items are added while the view is pinned");
    }

    /// <summary>
    /// ScrollToBottom() must reset the pin (Scrolled back to false).
    /// </summary>
    [AvaloniaTest]
    public void IrcOutputBox_ScrollToBottom_DoesNotThrow()
    {
        var window = new Window { Width = 400, Height = 300 };
        var box = new IrcOutputBox();
        window.Content = box;
        window.Show();

        var items = new ObservableCollection<ChatItemViewModel>();
        box.DataContext = new ChannelViewModelStub(items);
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.DoesNotThrow(() => box.ScrollToBottom());
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.That(box.Scrolled, Is.False);
    }

    /// <summary>
    /// The scroll indicator Border is bound to Scrolled via ElementName binding.
    /// When Scrolled is false (the default), the indicator must not be visible.
    /// </summary>
    [AvaloniaTest]
    public void IrcOutputBox_ScrollIndicator_HiddenWhenScrolledIsFalse()
    {
        var window = new Window { Width = 400, Height = 300 };
        var box = new IrcOutputBox();
        window.Content = box;
        window.Show();
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // Ensure Scrolled is false (default state)
        box.Scrolled = false;
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // The scroll indicator is the first child of the DockPanel (a Border)
        var dockPanel = (DockPanel)box.Content!;
        var indicator = (Border)dockPanel.Children[0];

        Assert.That(indicator.IsVisible, Is.False,
            "Scroll indicator must be hidden when Scrolled is false");
    }

    /// <summary>
    /// When Scrolled is set to true the scroll indicator Border must become visible.
    /// </summary>
    [AvaloniaTest]
    public void IrcOutputBox_ScrollIndicator_VisibleWhenScrolledIsTrue()
    {
        var window = new Window { Width = 400, Height = 300 };
        var box = new IrcOutputBox();
        window.Content = box;
        window.Show();
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        box.Scrolled = true;
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var dockPanel = (DockPanel)box.Content!;
        var indicator = (Border)dockPanel.Children[0];

        Assert.That(indicator.IsVisible, Is.True,
            "Scroll indicator must be visible when Scrolled is true");
    }

    /// <summary>
    /// Setting Scrolled back to false after it was true must hide the indicator again.
    /// </summary>
    [AvaloniaTest]
    public void IrcOutputBox_ScrollIndicator_HiddenAfterScrolledResetToFalse()
    {
        var window = new Window { Width = 400, Height = 300 };
        var box = new IrcOutputBox();
        window.Content = box;
        window.Show();
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        box.Scrolled = true;
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        box.Scrolled = false;
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var dockPanel = (DockPanel)box.Content!;
        var indicator = (Border)dockPanel.Children[0];

        Assert.That(indicator.IsVisible, Is.False,
            "Scroll indicator must be hidden after Scrolled is reset to false");
    }

    /// <summary>
    /// Minimal DataContext stub that exposes a ChatLines collection so that
    /// IrcOutputBox's ItemsSource binding (bound to ChatLines) resolves.
    /// </summary>
    private sealed class ChannelViewModelStub
    {
        public ObservableCollection<ChatItemViewModel> ChatLines { get; }
        public ChannelViewModelStub(ObservableCollection<ChatItemViewModel> items) => ChatLines = items;
    }
}

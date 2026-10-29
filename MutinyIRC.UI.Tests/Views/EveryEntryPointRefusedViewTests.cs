using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MutinyIRC.UI.ViewModels;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// Covers the Reconnect button that the stop template in App.axaml draws after every entry
/// point refused its certificate.
/// </summary>
[TestFixture]
public class EveryEntryPointRefusedViewTests
{
    /// <summary>
    /// Shows <paramref name="items"/> through the real chat line templates of App.axaml, which
    /// the test application does not load.
    /// </summary>
    private static Window Show(params ChatItemViewModel[] items)
    {
        var app = new App();
        app.Initialize();

        var window = new Window
        {
            Width = 600,
            Height = 400,
            Content = new ItemsControl { ItemsSource = items }
        };
        window.DataTemplates.AddRange(app.DataTemplates);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static Button[] ReconnectButtons(Window window) =>
        window.GetVisualDescendants().OfType<Button>().Where(b => b.Name == "reconnectButton").ToArray();

    [AvaloniaTest]
    public void AStop_ShowsTheMessageAndAReconnectButton()
    {
        Window window = Show(new EveryEntryPointRefusedViewModel(DateTime.Now, _ => { }));

        Button button = ReconnectButtons(window).Single();
        Assert.Multiple(() =>
        {
            Assert.That(button.IsEffectivelyVisible, Is.True);
            Assert.That(button.Content, Is.EqualTo("Reconnect"));
            Assert.That(button.IsEffectivelyEnabled, Is.True);
            Assert.That(window.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text),
                Does.Contain("Every server refused its certificate. Mutiny stopped trying. " +
                             "Trust a certificate, or reconnect."));
        });
    }

    [AvaloniaTest]
    public void TheReconnectButton_RunsTheAction()
    {
        int clicks = 0;
        Window window = Show(new EveryEntryPointRefusedViewModel(DateTime.Now, _ => clicks++));

        Button button = ReconnectButtons(window).Single();
        button.Command!.Execute(button.CommandParameter);
        Dispatcher.UIThread.RunJobs();

        Assert.That(clicks, Is.EqualTo(1));
    }

    [AvaloniaTest]
    public void ARetiredStop_HidesItsButton()
    {
        var stop = new EveryEntryPointRefusedViewModel(DateTime.Now, _ => { });
        Window window = Show(stop);

        stop.CanReconnect = false;
        Dispatcher.UIThread.RunJobs();

        Assert.That(ReconnectButtons(window).Single().IsEffectivelyVisible, Is.False);
    }
}

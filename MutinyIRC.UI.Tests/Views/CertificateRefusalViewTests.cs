using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.UI.Views;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// Covers the Trust button that the certificate refusal template in App.axaml draws, and the
/// dialog that confirms a pin replacement.
/// </summary>
[TestFixture]
public class CertificateRefusalViewTests
{
    private static CertificateRefusalViewModel Refusal(string? pinnedFingerprint = null) =>
        new(DateTime.Now, "Refused a.fake.com:6697", "a.fake.com", 6697,
            new string('B', 64), pinnedFingerprint, _ => Task.CompletedTask);

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

    private static Button[] TrustButtons(Window window) =>
        window.GetVisualDescendants().OfType<Button>().Where(b => b.Name == "trustCertificateButton").ToArray();

    [AvaloniaTest]
    public void ARefusalThatCanTrust_ShowsItsButton()
    {
        Window window = Show(Refusal());

        Button button = TrustButtons(window).Single();
        Assert.Multiple(() =>
        {
            Assert.That(button.IsEffectivelyVisible, Is.True);
            Assert.That(button.Content, Is.EqualTo("Trust this certificate"));
            Assert.That(button.IsEffectivelyEnabled, Is.True);
        });
    }

    [AvaloniaTest]
    public void ARetiredRefusal_HidesItsButton()
    {
        CertificateRefusalViewModel older = Refusal();
        Window window = Show(older, Refusal());

        older.CanTrust = false;
        Dispatcher.UIThread.RunJobs();

        Assert.That(TrustButtons(window).Select(b => b.IsEffectivelyVisible), Is.EqualTo(new[] { false, true }));
    }

    [AvaloniaTest]
    public void AChangedPinRefusal_LabelsTheButtonReplace()
    {
        Window window = Show(Refusal(pinnedFingerprint: new string('A', 64)));

        Assert.That(TrustButtons(window).Single().Content, Is.EqualTo("Replace pinned certificate"));
    }

    private static Task<bool> AskAndClick(string buttonName)
    {
        var owner = new Window();
        owner.Show();
        var dialog = new PinReplacementWindow(Refusal(pinnedFingerprint: new string('A', 64)));
        Task<bool> answer = dialog.ShowDialog<bool>(owner);
        Dispatcher.UIThread.RunJobs();

        Button button = dialog.GetVisualDescendants().OfType<Button>().Single(b => b.Name == buttonName);
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        return answer;
    }

    [AvaloniaTest]
    public async Task PinReplacementDialog_Replace_SaysYes()
    {
        Assert.That(await AskAndClick("replaceButton"), Is.True);
    }

    [AvaloniaTest]
    public async Task PinReplacementDialog_Cancel_SaysNo()
    {
        Assert.That(await AskAndClick("cancelButton"), Is.False);
    }
}

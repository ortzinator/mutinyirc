using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FlamingIRC;
using MutinyIRC.UI.ViewModels;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// Covers the "Show timestamps" setting against the real chat line templates of App.axaml. A
/// change to the setting must also apply to the lines that are already shown.
/// </summary>
[TestFixture]
public class ChatLineTimestampTests
{
    // The test looks for the time text that the templates write with the 'HH:mm' format.
    private static readonly DateTime LineTime = new(2026, 10, 31, 13, 57, 0);
    private const string LineTimeText = "13:57";

    private bool _originalShowTimestamps;

    [SetUp]
    public void SetUp() => _originalShowTimestamps = AppSettings.Instance.ShowTimestamps;

    [TearDown]
    public void TearDown() => AppSettings.Instance.ShowTimestamps = _originalShowTimestamps;

    /// <summary>One line of each type that has its own template in App.axaml.</summary>
    private static ChatItemViewModel[] OneLineOfEachTemplate()
    {
        var user = new User { Nick = "alice" };
        return
        [
            new ChannelActionViewModel(LineTime, "waves", user),
            new PrivateNoticeViewModel(LineTime, "psst", "alice"),
            new OutgoingNoticeViewModel(LineTime, "hi", "bob"),
            new ChannelMessageViewModel(LineTime, "hello", user),
            new IrcErrorViewModel(LineTime, "No such nick", "401"),
            new CertificateRefusalViewModel(LineTime, "Refused a.fake.com:6697", "a.fake.com", 6697,
                new string('B', 64), null, _ => Task.CompletedTask),
            new EveryEntryPointRefusedViewModel(LineTime, _ => { }),
            new ErrorMessageViewModel(LineTime, "Error"),
            new WarningMessageViewModel(LineTime, "Warning"),
            new ChatItemViewModel(LineTime, "Status")
        ];
    }

    private static Window Show(ChatItemViewModel[] items)
    {
        var app = new App();
        app.Initialize();

        var window = new Window
        {
            Width = 600,
            Height = 800,
            Content = new ItemsControl { ItemsSource = items }
        };
        window.DataTemplates.AddRange(app.DataTemplates);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static TextBlock[] Timestamps(Window window) =>
        window.GetVisualDescendants().OfType<TextBlock>()
            .Where(tb => tb.Text == LineTimeText)
            .ToArray();

    [AvaloniaTest]
    public void TurningTimestampsOff_HidesThemOnLinesAlreadyShown()
    {
        AppSettings.Instance.ShowTimestamps = true;
        ChatItemViewModel[] items = OneLineOfEachTemplate();
        Window window = Show(items);

        TextBlock[] timestamps = Timestamps(window);
        Assert.That(timestamps, Has.Length.EqualTo(items.Length),
            "Each template must draw the line's time");
        Assert.That(timestamps.All(tb => tb.IsVisible), Is.True,
            "Timestamps must show while the setting is on");

        AppSettings.Instance.ShowTimestamps = false;
        Dispatcher.UIThread.RunJobs();

        Assert.That(timestamps.Where(tb => tb.IsVisible), Is.Empty,
            "Turning the setting off must hide the time on lines already shown");
    }

    [AvaloniaTest]
    public void TurningTimestampsOn_ShowsThemOnLinesAlreadyShown()
    {
        AppSettings.Instance.ShowTimestamps = false;
        ChatItemViewModel[] items = OneLineOfEachTemplate();
        Window window = Show(items);

        TextBlock[] timestamps = Timestamps(window);
        Assert.That(timestamps, Has.Length.EqualTo(items.Length),
            "Each template must draw the line's time");
        Assert.That(timestamps.Where(tb => tb.IsVisible), Is.Empty,
            "Timestamps must stay hidden while the setting is off");

        AppSettings.Instance.ShowTimestamps = true;
        Dispatcher.UIThread.RunJobs();

        Assert.That(timestamps.All(tb => tb.IsVisible), Is.True,
            "Turning the setting on must show the time on lines already shown");
    }
}

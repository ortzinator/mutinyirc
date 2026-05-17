using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using NUnit.Framework;
using OrtzIRC.Avalonia.Converters;
using OrtzIRC.Avalonia.ViewModels;

namespace MutinyIRC.Avalonia.Tests.Views;

/// <summary>
/// Verifies the visual properties of the ChannelActionViewModel message template.
/// The Run structure built in BuildAndShow must be kept in sync with the
/// ChannelActionViewModel DataTemplate in App.axaml.
/// </summary>
[TestFixture]
public class ChannelActionTemplateTests
{
    private static readonly NickColorConverter NickConverter = new();

    private static IBrush ExpectedNickColor(string nick) =>
        (IBrush)NickConverter.Convert(nick, typeof(IBrush), null!, CultureInfo.InvariantCulture)!;

    [AvaloniaTest]
    public void ChannelActionTemplate_TextBlock_IsItalic()
    {
        var (_, tb) = BuildAndShow("alice", "waves");
        Assert.That(tb.FontStyle, Is.EqualTo(FontStyle.Italic));
    }

    [AvaloniaTest]
    public void ChannelActionTemplate_AsteriskRun_UsesNickColor()
    {
        const string nick = "alice";
        var (_, tb) = BuildAndShow(nick, "waves");
        var run = tb.Inlines!.OfType<Run>().ElementAt(0);

        Assert.That(run.Text, Does.StartWith("*"),
            "First run must be the asterisk prefix");
        Assert.That(run.Foreground?.ToString(), Is.EqualTo(ExpectedNickColor(nick).ToString()),
            "Asterisk run must carry the nick's hash color");
    }

    [AvaloniaTest]
    public void ChannelActionTemplate_NickRun_UsesNickColorAndIsSemiBold()
    {
        const string nick = "alice";
        var (_, tb) = BuildAndShow(nick, "waves");
        var nickRun = tb.Inlines!.OfType<Run>().ElementAt(1);

        Assert.That(nickRun.Text, Is.EqualTo(nick),
            "Second run must contain the nick");
        Assert.That(nickRun.Foreground?.ToString(), Is.EqualTo(ExpectedNickColor(nick).ToString()),
            "Nick run must carry the nick's hash color");
        Assert.That(nickRun.FontWeight, Is.EqualTo(FontWeight.SemiBold));
    }

    [AvaloniaTest]
    public void ChannelActionTemplate_MessageRun_InheritsDefaultForeground()
    {
        const string nick = "alice";
        const string message = "waves hello";
        var (_, tb) = BuildAndShow(nick, message);
        var messageRun = tb.Inlines!.OfType<Run>().ElementAt(3);

        Assert.That(messageRun.Text, Is.EqualTo(message));
        Assert.That(messageRun.Foreground?.ToString(),
            Is.Not.EqualTo(ExpectedNickColor(nick).ToString()),
            "Message run must not carry the nick color; it should inherit the default foreground");
    }

    /// <summary>
    /// Builds the TextBlock with the exact Run structure from App.axaml's
    /// ChannelActionViewModel DataTemplate and shows it in a Window so bindings resolve.
    /// </summary>
    private static (string nick, TextBlock tb) BuildAndShow(string nick, string message)
    {
        var vm = new ChannelActionViewModel(DateTime.Now, message, nick);

        var asteriskRun = new Run { Text = "* " };
        var nickRun = new Run { FontWeight = FontWeight.SemiBold };
        var spaceRun = new Run { Text = " " };
        var messageRun = new Run();

        var tb = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontStyle = FontStyle.Italic,
            DataContext = vm
        };
        tb.Inlines!.Add(asteriskRun);
        tb.Inlines.Add(nickRun);
        tb.Inlines.Add(spaceRun);
        tb.Inlines.Add(messageRun);

        asteriskRun.Bind(Run.ForegroundProperty,
            new Binding("User.Nick") { Converter = NickConverter });
        nickRun.Bind(Run.TextProperty, new Binding("User.Nick"));
        nickRun.Bind(Run.ForegroundProperty,
            new Binding("User.Nick") { Converter = NickConverter });
        messageRun.Bind(Run.TextProperty, new Binding("Message"));

        var window = new Window { Content = tb, Width = 400, Height = 100 };
        window.Show();
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        return (nick, tb);
    }
}

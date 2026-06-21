using Avalonia.Headless.NUnit;
using Avalonia.Input;
using NUnit.Framework;
using MutinyIRC.UI.Controls;
using MutinyIRC.UI.ViewModels;
using static MutinyIRC.UI.Tests.Controls.CommandTextBoxHarness;

namespace MutinyIRC.UI.Tests.Controls;

/// <summary>
/// Verifies Tab nickname completion in <see cref="CommandTextBox"/>. The control reads candidate
/// nicks from its <see cref="IrcViewModel.CompletionCandidates"/> DataContext, completes the token
/// at the caret, cycles through matches on repeated Tab, and swallows the Tab so focus stays put.
/// </summary>
[TestFixture]
public class CommandTextBoxCompletionTests
{
    private static bool PressTab(CommandTextBox box) => PressKey(box, Key.Tab);

    [AvaloniaTest]
    public void Tab_AtStartOfLine_CompletesWithColonSuffix()
    {
        var (box, _) = Host(new StubViewModel("alice"));
        box.Text = "ali";
        box.CaretIndex = 3;

        PressTab(box);

        Assert.That(box.Text, Is.EqualTo("alice: "));
        Assert.That(box.CaretIndex, Is.EqualTo("alice: ".Length));
    }

    [AvaloniaTest]
    public void Tab_MidLine_CompletesWithSpaceSuffix()
    {
        var (box, _) = Host(new StubViewModel("alice"));
        box.Text = "hi al";
        box.CaretIndex = 5;

        PressTab(box);

        Assert.That(box.Text, Is.EqualTo("hi alice "));
        Assert.That(box.CaretIndex, Is.EqualTo("hi alice ".Length));
    }

    [AvaloniaTest]
    public void Tab_IsCaseInsensitive()
    {
        var (box, _) = Host(new StubViewModel("Alice"));
        box.Text = "ali";
        box.CaretIndex = 3;

        PressTab(box);

        Assert.That(box.Text, Is.EqualTo("Alice: "));
    }

    [AvaloniaTest]
    public void Tab_RepeatedlyCyclesThroughMatches()
    {
        var (box, _) = Host(new StubViewModel("alice", "alan"));
        box.Text = "al";
        box.CaretIndex = 2;

        PressTab(box);
        Assert.That(box.Text, Is.EqualTo("alice: "));

        PressTab(box);
        Assert.That(box.Text, Is.EqualTo("alan: "));

        // Wraps back to the first match.
        PressTab(box);
        Assert.That(box.Text, Is.EqualTo("alice: "));
    }

    [AvaloniaTest]
    public void Tab_WithNoMatch_LeavesTextUnchanged()
    {
        var (box, _) = Host(new StubViewModel("bob"));
        box.Text = "xyz";
        box.CaretIndex = 3;

        bool handled = PressTab(box);

        Assert.That(box.Text, Is.EqualTo("xyz"));
        Assert.That(handled, Is.True, "Tab must still be swallowed so focus doesn't move");
    }

    [AvaloniaTest]
    public void Tab_OnWhitespaceToken_DoesNothing()
    {
        var (box, _) = Host(new StubViewModel("alice"));
        box.Text = "hi ";
        box.CaretIndex = 3;

        PressTab(box);

        Assert.That(box.Text, Is.EqualTo("hi "));
    }

    [AvaloniaTest]
    public void Tab_AfterEdit_RecomputesInsteadOfCycling()
    {
        var (box, _) = Host(new StubViewModel("alice", "alan"));
        box.Text = "al";
        box.CaretIndex = 2;

        PressTab(box);
        Assert.That(box.Text, Is.EqualTo("alice: "));

        // A non-Tab key resets the cycle; the next Tab starts a fresh completion from the new token.
        box.RaiseEvent(new KeyEventArgs { Key = Key.A, RoutedEvent = InputElement.KeyDownEvent });
        box.Text = "ala";
        box.CaretIndex = 3;

        PressTab(box);
        Assert.That(box.Text, Is.EqualTo("alan: "));
    }

    [AvaloniaTest]
    public void Tab_IsAlwaysHandled()
    {
        var (box, _) = Host(new StubViewModel("alice"));
        box.Text = "ali";
        box.CaretIndex = 3;

        Assert.That(PressTab(box), Is.True);
    }
}

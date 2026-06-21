using Avalonia.Headless.NUnit;
using Avalonia.Input;
using NUnit.Framework;
using MutinyIRC.UI.Controls;
using static MutinyIRC.UI.Tests.Controls.CommandTextBoxHarness;

namespace MutinyIRC.UI.Tests.Controls;

/// <summary>
/// Verifies command-history navigation in <see cref="CommandTextBox"/>. Submitting a non-blank line
/// pushes it onto the history; Up walks backward through earlier entries and Down walks forward,
/// returning to a blank line at the end. Re-submitting a recalled entry must not duplicate it.
/// </summary>
[TestFixture]
public class CommandTextBoxHistoryTests
{
    /// <summary>Types <paramref name="text"/> and submits it via Enter, as the user would.</summary>
    private static void Submit(CommandTextBox box, string text)
    {
        box.Text = text;
        box.CaretIndex = text.Length;
        PressKey(box, Key.Enter);
    }

    [AvaloniaTest]
    public void Submit_ClearsTheInput()
    {
        var (box, _) = Host();

        Submit(box, "hello");

        Assert.That(box.Text, Is.Empty);
    }

    [AvaloniaTest]
    public void Up_OnEmptyHistory_DoesNothing()
    {
        var (box, _) = Host();
        box.Text = "typing";
        box.CaretIndex = 6;

        PressKey(box, Key.Up);

        Assert.That(box.Text, Is.EqualTo("typing"));
    }

    [AvaloniaTest]
    public void Up_RecallsLastSubmittedCommand()
    {
        var (box, _) = Host();
        Submit(box, "foo");

        PressKey(box, Key.Up);

        Assert.That(box.Text, Is.EqualTo("foo"));
        Assert.That(box.CaretIndex, Is.EqualTo("foo".Length), "caret should sit at the end of the recalled text");
    }

    [AvaloniaTest]
    public void Up_WalksBackwardThroughHistory()
    {
        var (box, _) = Host();
        Submit(box, "one");
        Submit(box, "two");
        Submit(box, "three");

        PressKey(box, Key.Up);
        Assert.That(box.Text, Is.EqualTo("three"));

        PressKey(box, Key.Up);
        Assert.That(box.Text, Is.EqualTo("two"));

        PressKey(box, Key.Up);
        Assert.That(box.Text, Is.EqualTo("one"));
    }

    [AvaloniaTest]
    public void Up_StopsAtOldestEntry()
    {
        var (box, _) = Host();
        Submit(box, "one");
        Submit(box, "two");

        PressKey(box, Key.Up);   // two
        PressKey(box, Key.Up);   // one
        PressKey(box, Key.Up);   // stays on one

        Assert.That(box.Text, Is.EqualTo("one"), "Up past the oldest entry is a no-op");
    }

    [AvaloniaTest]
    public void Down_AfterUp_WalksForwardThroughHistory()
    {
        var (box, _) = Host();
        Submit(box, "one");
        Submit(box, "two");
        Submit(box, "three");

        PressKey(box, Key.Up);   // three
        PressKey(box, Key.Up);   // two
        PressKey(box, Key.Up);   // one

        PressKey(box, Key.Down);
        Assert.That(box.Text, Is.EqualTo("two"));

        PressKey(box, Key.Down);
        Assert.That(box.Text, Is.EqualTo("three"));
    }

    [AvaloniaTest]
    public void Down_PastNewestEntry_ClearsToBlankLine()
    {
        var (box, _) = Host();
        Submit(box, "one");
        Submit(box, "two");

        PressKey(box, Key.Up);     // two (newest)
        PressKey(box, Key.Down);   // step off the end → blank line

        Assert.That(box.Text, Is.Empty);
    }

    [AvaloniaTest]
    public void Down_OnUnsentDraft_StashesItIntoHistory()
    {
        var (box, _) = Host();
        box.Text = "draft";
        box.CaretIndex = 5;

        // Down at the end of history with a non-blank draft tucks it onto the history and clears.
        PressKey(box, Key.Down);
        Assert.That(box.Text, Is.Empty);

        // The stashed draft is now the newest history entry.
        PressKey(box, Key.Up);
        Assert.That(box.Text, Is.EqualTo("draft"));
    }

    [AvaloniaTest]
    public void Submit_OfRecalledEntry_DoesNotDuplicateIt()
    {
        var (box, _) = Host();
        Submit(box, "foo");
        Submit(box, "bar");

        // Recall "bar" and submit it unchanged.
        PressKey(box, Key.Up);
        Assert.That(box.Text, Is.EqualTo("bar"));
        PressKey(box, Key.Enter);

        // History should still be just [foo, bar], not [foo, bar, bar].
        PressKey(box, Key.Up);
        Assert.That(box.Text, Is.EqualTo("bar"));
        PressKey(box, Key.Up);
        Assert.That(box.Text, Is.EqualTo("foo"));
    }

    [AvaloniaTest]
    public void Submit_OfEditedRecalledEntry_ReplacesTheOriginal()
    {
        var (box, _) = Host();
        Submit(box, "foo");

        // Recall "foo", edit it, and submit the edited text.
        PressKey(box, Key.Up);
        box.Text = "food";
        box.CaretIndex = 4;
        PressKey(box, Key.Enter);

        // The original "foo" is gone; only the edited "food" remains.
        PressKey(box, Key.Up);
        Assert.That(box.Text, Is.EqualTo("food"));
        PressKey(box, Key.Up);
        Assert.That(box.Text, Is.EqualTo("food"), "there should be only one history entry");
    }

    [AvaloniaTest]
    public void Submit_OfBlankLine_IsIgnored()
    {
        var (box, _) = Host();
        Submit(box, "real");

        Submit(box, "   ");   // whitespace-only: must not enter history

        PressKey(box, Key.Up);
        Assert.That(box.Text, Is.EqualTo("real"));
    }
}

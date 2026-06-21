namespace MutinyIRC.UI.Controls;

using System;
using System.Collections.Generic;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using ViewModels;

public class CommandTextBox : TextBox
{
    private int historyIndex;
    private List<string> cmdHistory;

    // ── Tab-completion cycle state ──
    // Set when a Tab completion is in progress so repeated Tabs cycle through matches in place.
    // Any other key, a submit, or history navigation clears it via ResetCompletion().
    private int completionStart;        // index in the text where the completed token begins
    private List<string>? completionMatches;
    private int completionIndex;        // which match is currently inserted
    private bool completing;            // guards against our own Selection/Text edits resetting state

    /// <summary>Appended after a completed nick when it's the first word on the line.</summary>
    private const string StartOfLineSuffix = ": ";

    public event EventHandler<CommandEventArgs>? CommandEntered;

    protected override Type StyleKeyOverride => typeof(TextBox);

    public CommandTextBox()
    {
        cmdHistory = new List<string>(40);
        AddHandler(KeyDownEvent, HandleKeyDown, handledEventsToo: true);
    }

    public void Submit()
    {
        ResetCompletion();
        var text = Text;
        if (!string.IsNullOrWhiteSpace(text))
        {
            if (historyIndex != cmdHistory.Count)
                cmdHistory.RemoveAt(historyIndex);
            cmdHistory.Add(text);

            var vm = DataContext as IrcViewModel;
            vm?.ExecuteCommand.Execute(text);

            CommandEntered?.Invoke(this, new CommandEventArgs(text));
            Clear();
            historyIndex = cmdHistory.Count;
        }
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        // A Tab keeps (or starts) the completion cycle; anything else ends it so the next Tab
        // recomputes matches from the current text.
        if (e.Key != Key.Tab)
            ResetCompletion();

        switch (e.Key)
        {
            case Key.Tab:
                CompleteNick();
                e.Handled = true;   // don't let Tab move focus out of the input box
                break;

            case Key.Up:
                if (historyIndex > 0)
                {
                    historyIndex--;
                    Text = cmdHistory[historyIndex];
                    CaretIndex = Text.Length;
                    e.Handled = true;
                }
                break;

            case Key.Down:
                var text = Text;
                if (historyIndex == cmdHistory.Count && !string.IsNullOrWhiteSpace(text))
                {
                    cmdHistory.Add(text);
                    historyIndex = cmdHistory.Count;
                    Clear();
                    e.Handled = true;
                }
                else if (historyIndex == cmdHistory.Count - 1)
                {
                    historyIndex++;
                    Clear();
                    e.Handled = true;
                }
                else if (historyIndex < cmdHistory.Count)
                {
                    historyIndex++;
                    Text = cmdHistory[historyIndex];
                    CaretIndex = Text.Length;
                    e.Handled = true;
                }
                break;

            case Key.Enter:
                Submit();
                e.Handled = true;
                break;
        }
    }

    /// <summary>
    ///   Completes the nickname being typed at the caret. The first Tab replaces the partial token
    ///   with the first matching nick; each further Tab cycles to the next match in place.
    /// </summary>
    private void CompleteNick()
    {
        // Already mid-cycle: advance to the next match.
        if (completionMatches != null)
        {
            completionIndex = (completionIndex + 1) % completionMatches.Count;
            ApplyCompletion(completionMatches[completionIndex]);
            return;
        }

        var text = Text ?? string.Empty;
        int caret = Math.Clamp(CaretIndex, 0, text.Length);

        // Walk back from the caret to the start of the token being completed.
        int start = caret;
        while (start > 0 && !char.IsWhiteSpace(text[start - 1]))
            start--;

        string prefix = text.Substring(start, caret - start);
        if (prefix.Length == 0)
            return;   // nothing to complete (caret on whitespace or empty line)

        var candidates = (DataContext as IrcViewModel)?.CompletionCandidates;
        if (candidates == null || candidates.Count == 0)
            return;

        var matches = new List<string>();
        foreach (var nick in candidates)
            if (nick.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                matches.Add(nick);

        if (matches.Count == 0)
            return;

        completionStart = start;
        completionMatches = matches;
        completionIndex = 0;
        ApplyCompletion(matches[0]);
    }

    /// <summary>
    ///   Replaces the token from <see cref="completionStart"/> through the current caret with
    ///   <paramref name="nick"/>, adding the start-of-line suffix or a trailing space as appropriate.
    /// </summary>
    private void ApplyCompletion(string nick)
    {
        var text = Text ?? string.Empty;
        int caret = Math.Clamp(CaretIndex, completionStart, text.Length);

        string suffix = completionStart == 0 ? StartOfLineSuffix : " ";
        string replacement = nick + suffix;

        completing = true;
        Text = text.Substring(0, completionStart) + replacement + text.Substring(caret);
        CaretIndex = completionStart + replacement.Length;
        completing = false;
    }

    private void ResetCompletion()
    {
        if (completing)
            return;   // don't tear down state while we're applying our own edit

        completionMatches = null;
        completionIndex = 0;
    }
}

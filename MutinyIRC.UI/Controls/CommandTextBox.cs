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

    public event EventHandler<CommandEventArgs>? CommandEntered;

    protected override Type StyleKeyOverride => typeof(TextBox);

    public CommandTextBox()
    {
        cmdHistory = new List<string>(40);
        AddHandler(KeyDownEvent, HandleKeyDown, handledEventsToo: true);
    }

    public void Submit()
    {
        if (Text?.Trim() != string.Empty)
        {
            if (historyIndex != cmdHistory.Count)
                cmdHistory.RemoveAt(historyIndex);
            cmdHistory.Add(Text);

            var vm = DataContext as IrcViewModel;
            vm?.ExecuteCommand.Execute(Text);

            CommandEntered?.Invoke(this, new CommandEventArgs(Text));
            Clear();
            historyIndex = cmdHistory.Count;
        }
    }

    private void HandleKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
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
                if (historyIndex == cmdHistory.Count && Text?.Trim() != string.Empty)
                {
                    cmdHistory.Add(Text);
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
}

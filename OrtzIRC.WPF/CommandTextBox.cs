namespace OrtzIRC.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using OrtzIRC.WPF.ViewModels;

    public class CommandTextBox : TextBox
    {
        private int historyIndex;
        private List<string> cmdHistory;

        public static readonly RoutedEvent CommandEnteredEvent =
            EventManager.RegisterRoutedEvent("CommandEntered", RoutingStrategy.Bubble,
                                             typeof(EventHandler<CommandEventArgs>), typeof(CommandTextBox));

        public event EventHandler<CommandEventArgs> CommandEntered
        {
            add { AddHandler(CommandEnteredEvent, value); }
            remove { RemoveHandler(CommandEnteredEvent, value); }
        }

        public CommandTextBox()
        {
            cmdHistory = new List<string>(40);
            AddHandler(CommandTextBox.KeyDownEvent, new RoutedEventHandler(HandleHandledKeyDown), true);
        }

        static CommandTextBox()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandTextBox), new FrameworkPropertyMetadata(typeof(CommandTextBox)));
        }

        public void HandleHandledKeyDown(object sender, RoutedEventArgs e)
        {
            KeyEventArgs ke = e as KeyEventArgs;
            if (ke == null) return;

            switch (ke.Key)
            {
                case Key.Up:
                    if (historyIndex > 0)
                    {
                        historyIndex--;
                        Text = cmdHistory[historyIndex];
                        e.Handled = true;
                    }
                    break;
                case Key.Down:
                    if (historyIndex == cmdHistory.Count && Text.Trim() != string.Empty)
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
                        e.Handled = true;
                    }
                    break;
                case Key.Enter:
                    if (Text.Trim() != string.Empty)
                    {
                        if (historyIndex != cmdHistory.Count)
                        {
                            cmdHistory.RemoveAt(historyIndex);
                        }
                        cmdHistory.Add(Text);

                        var vm = DataContext as ChannelViewModel;
                        vm.ExecuteCommand.Execute(Text);

                        RaiseEvent(new CommandEventArgs(CommandEnteredEvent, Text));
                        Clear();
                        historyIndex = cmdHistory.Count;
                        e.Handled = true;
                    }
                    break;
            }
        }
    }
}

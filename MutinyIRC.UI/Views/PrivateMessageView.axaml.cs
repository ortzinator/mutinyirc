namespace MutinyIRC.UI.Views;

using global::Avalonia.Controls;

public partial class PrivateMessageView : UserControl
{
    public PrivateMessageView()
    {
        InitializeComponent();
        commandBox.CommandEntered += (_, _) => outputBox.ScrollToBottom();
        sendButton.Click += (_, _) =>
        {
            commandBox.Submit();
            commandBox.Focus();
        };
    }
}

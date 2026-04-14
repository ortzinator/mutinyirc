namespace OrtzIRC.Avalonia.Views;

using global::Avalonia.Controls;

public partial class ServerView : UserControl
{
    public ServerView()
    {
        InitializeComponent();
        sendButton.Click += (_, _) =>
        {
            commandBox.Submit();
            commandBox.Focus();
        };
    }
}

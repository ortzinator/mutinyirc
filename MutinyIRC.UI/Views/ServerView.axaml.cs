using global::Avalonia.Controls;

namespace MutinyIRC.UI.Views;

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

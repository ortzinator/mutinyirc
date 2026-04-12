namespace OrtzIRC.Avalonia.Views;

using global::Avalonia.Controls;
using OrtzIRC.Avalonia.Controls;

public partial class ChannelView : UserControl
{
    public ChannelView()
    {
        InitializeComponent();
        commandBox.CommandEntered += (_, _) => outputBox.ScrollToBottom();
    }
}

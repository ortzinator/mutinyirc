using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using MutinyIRC.UI.ViewModels;

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

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // A changed pinned certificate replaces the pin only after the user confirms it here.
        if (DataContext is ServerViewModel server)
            server.ConfirmPinReplacement = ConfirmPinReplacement;
    }

    private Task<bool> ConfirmPinReplacement(CertificateRefusalViewModel refusal)
    {
        // With no window to own the dialog, nothing can ask, so the pin stays.
        if (TopLevel.GetTopLevel(this) is not Window owner)
            return Task.FromResult(false);

        return new PinReplacementWindow(refusal).ShowDialog<bool>(owner);
    }
}

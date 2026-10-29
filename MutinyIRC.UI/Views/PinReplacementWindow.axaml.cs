using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MutinyIRC.Common;
using MutinyIRC.UI.Resources;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Views;

/// <summary>
/// Asks the user to confirm that a changed certificate replaces the pin of its entry point.
/// <see cref="Window.ShowDialog{TResult}(Window)"/> gives true for Replace, and false for Cancel
/// or for a closed window.
/// </summary>
public partial class PinReplacementWindow : Window
{
    // The XAML loader needs a constructor with no parameters.
    public PinReplacementWindow()
    {
        InitializeComponent();
        Title = ServerStrings.ReplacePinTitle;
        replaceButton.Content = ServerStrings.ReplacePinConfirmButton;
        cancelButton.Content = ServerStrings.ReplacePinCancelButton;
    }

    /// <param name="refusal">
    /// A refusal whose <see cref="CertificateRefusalViewModel.ReplacesPin"/> is true.
    /// </param>
    /// <exception cref="ArgumentException">The refusal replaces no pin.</exception>
    public PinReplacementWindow(CertificateRefusalViewModel refusal) : this()
    {
        // The window opens only when the refusal replaces a pin, so the pin is always there.
        string pinnedFingerprint = refusal.PinnedFingerprint
            ?? throw new ArgumentException("The refusal replaces no pin.", nameof(refusal));

        messageText.Text = ServerStrings.ReplacePinMessage.With(refusal.Host, refusal.Port);
        fingerprintsText.Text = string.Join("\n",
            ServerStrings.CertificatePinnedLine.With(
                CertificateRefusal.FormatFingerprint(pinnedFingerprint)),
            ServerStrings.CertificateNowLine.With(CertificateRefusal.FormatFingerprint(refusal.Fingerprint)));
    }

    private void ReplaceButton_Click(object? sender, RoutedEventArgs e) => Close(true);

    private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(false);
}

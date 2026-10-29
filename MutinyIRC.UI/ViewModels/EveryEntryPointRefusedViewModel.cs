using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutinyIRC.UI.Resources;

namespace MutinyIRC.UI.ViewModels;

/// <summary>
/// The error block that says every entry point refused its certificate, with the button that
/// starts a new connect attempt from the first entry point. Only the newest block of a
/// connection can reconnect: the server window sets <see cref="CanReconnect"/> to false on
/// older blocks.
/// </summary>
public partial class EveryEntryPointRefusedViewModel : ErrorMessageViewModel
{
    private readonly Action<EveryEntryPointRefusedViewModel> _reconnect;

    /// <param name="reconnect">What the button does.</param>
    public EveryEntryPointRefusedViewModel(DateTime time,
                                           Action<EveryEntryPointRefusedViewModel> reconnect)
        : base(time, ServerStrings.EveryEntryPointRefusedMessage)
    {
        _reconnect = reconnect;
    }

    public string ReconnectLabel => ServerStrings.ReconnectButton;

    /// <summary>
    /// Whether the button shows and works. It is false when a newer block came, when the
    /// connection registered, or when the button was used.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReconnectCommand))]
    private bool _canReconnect = true;

    [RelayCommand(CanExecute = nameof(CanReconnect))]
    private void Reconnect() => _reconnect(this);
}

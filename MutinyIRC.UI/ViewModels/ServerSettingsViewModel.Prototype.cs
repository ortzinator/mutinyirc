using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MutinyIRC.Common;
using MutinyIRC.UI.ViewModels.Prototype;

namespace MutinyIRC.UI.ViewModels;

/// <summary>PROTOTYPE, throw away: the draft that the edit-and-move variants edit.</summary>
public partial class ServerSettingsViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInlineEditing), nameof(PinWillClear))]
    private bool _isEditing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PinWillClear))]
    private string _draftHost = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PinWillClear))]
    private string _draftPort = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PinWillClear), nameof(DraftPortPlaceholder))]
    private bool _draftSsl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMoving))]
    private NetworkSettingsViewModel? _draftNetwork;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRefusal))]
    private string? _refusal;

    public static EntryPointEditPrototype Proto => EntryPointEditPrototype.Instance;

    public ObservableCollection<NetworkSettingsViewModel>? AllNetworks => Proto.Settings?.Networks;

    public bool IsInlineEditing => IsEditing && Proto.IsA;

    public bool HasRefusal => Refusal is not null;

    public string DraftPortPlaceholder => (DraftSsl ? IrcPorts.DefaultTlsPort : IrcPorts.DefaultPlainPort).ToString();

    public bool IsMoving => DraftNetwork is not null && DraftNetwork != Proto.OwnerOf(this);

    /// <summary>Whether applying the draft would clear the pin: a new host or port, or TLS off.</summary>
    public bool PinWillClear =>
        IsPinned && IsEditing &&
        (!string.Equals(DraftHost.Trim(), Url, StringComparison.OrdinalIgnoreCase)
         || (DraftPort.Trim().Length > 0 && DraftPort.Trim() != Port.ToString())
         || !DraftSsl);

    public void BeginDraft()
    {
        DraftHost = Url;
        DraftPort = Port.ToString();
        DraftSsl = Ssl;
        DraftNetwork = Proto.OwnerOf(this);
        Refusal = null;
        IsEditing = true;
        OnPropertyChanged(nameof(AllNetworks));
    }

    public void CancelDraft()
    {
        IsEditing = false;
        Refusal = null;
    }

    partial void OnDraftHostChanged(string value) => Refusal = null;
    partial void OnDraftPortChanged(string value) => Refusal = null;
}

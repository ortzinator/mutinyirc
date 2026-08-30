using MutinyIRC.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MutinyIRC.UI.ViewModels;

/// <summary>
/// One autojoin chip. Membership of <see cref="NetworkSettingsViewModel.Channels"/> is the
/// autojoin flag, so this carries no flag of its own.
/// </summary>
public partial class ChannelSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsKeyed))]
    private string _key = string.Empty;

    [ObservableProperty] private string _name = string.Empty;

    /// <summary>Whether the channel needs a key, which is what puts a padlock on the chip.</summary>
    public bool IsKeyed => !string.IsNullOrEmpty(Key);

    public ChannelSettingsViewModel() { }

    public ChannelSettingsViewModel(ChannelSettings model)
    {
        _name = model.Name ?? string.Empty;
        _key = model.Key ?? string.Empty;
    }

    public ChannelSettings ToModel() => new(Name, true, string.Empty, Key);
}

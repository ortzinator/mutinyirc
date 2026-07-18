
using MutinyIRC.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MutinyIRC.UI.ViewModels;

public partial class ChannelSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _key = string.Empty;
    [ObservableProperty] private bool _autoJoin;

    public ChannelSettingsViewModel() { }

    public ChannelSettingsViewModel(ChannelSettings model)
    {
        _name = model.Name ?? string.Empty;
        _key = model.Key ?? string.Empty;
        _autoJoin = model.AutoJoin;
    }

    public ChannelSettings ToModel()
    {
        return new ChannelSettings(Name, AutoJoin, string.Empty, Key);
    }
}

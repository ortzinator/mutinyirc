using CommunityToolkit.Mvvm.ComponentModel;

namespace MutinyIRC.UI.ViewModels;

/// <summary>
/// One row of the servers table: a host, its port, and whether that port speaks TLS.
/// </summary>
public partial class ServerSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private int _port = ServerSettings.DefaultPort;
    [ObservableProperty] private bool _ssl;

    // Position in the failover order. The owning network writes both whenever the order
    // changes, because a row cannot see the list it is in.
    [ObservableProperty] private int _order;
    [ObservableProperty] private bool _isFirst;

    public ServerSettingsViewModel() { }

    public ServerSettingsViewModel(ServerSettings model)
    {
        _url = model.Url ?? string.Empty;
        _port = model.Port;
        _ssl = model.Ssl;
    }

    public ServerSettings ToModel() => new(Url, Port, Ssl);
}

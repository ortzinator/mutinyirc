namespace MutinyIRC.UI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public partial class ServerSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _ports = "6667";
    [ObservableProperty] private string _nick = string.Empty;
    [ObservableProperty] private bool _ssl;
    [ObservableProperty] private bool _autoConnect;

    public ServerSettingsViewModel() { }

    public ServerSettingsViewModel(ServerSettings model)
    {
        _url = model.Url ?? string.Empty;
        _description = model.Description ?? string.Empty;
        _ports = model.Ports ?? "6667";
        _nick = model.Nick ?? string.Empty;
        _ssl = model.Ssl;
        _autoConnect = model.AutoConnect;
    }

    public ServerSettings ToModel()
    {
        return new ServerSettings(Url, Description, Ports, Ssl)
        {
            Nick = Nick,
            AutoConnect = AutoConnect,
        };
    }
}

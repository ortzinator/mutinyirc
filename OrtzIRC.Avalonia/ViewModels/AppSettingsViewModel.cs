namespace OrtzIRC.Avalonia.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public partial class AppSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _showTimestamps;
    [ObservableProperty] private string _firstNick = null!;
    [ObservableProperty] private string _secondNick = null!;
    [ObservableProperty] private string _thirdNick = null!;
    [ObservableProperty] private bool _loggerActivated;
    [ObservableProperty] private bool _loggerTimestampsActivated;
    [ObservableProperty] private string _loggerTimestampFormat = null!;

    public AppSettingsViewModel()
    {
        var s = AppSettings.Instance;
        ShowTimestamps = s.ShowTimestamps;
        FirstNick = s.FirstNick;
        SecondNick = s.SecondNick;
        ThirdNick = s.ThirdNick;
        LoggerActivated = s.LoggerActivated;
        LoggerTimestampsActivated = s.LoggerTimestampsActivated;
        LoggerTimestampFormat = s.LoggerTimestampFormat;
    }

    public void Save()
    {
        var s = AppSettings.Instance;
        s.ShowTimestamps = ShowTimestamps;
        s.FirstNick = FirstNick;
        s.SecondNick = SecondNick;
        s.ThirdNick = ThirdNick;
        s.LoggerActivated = LoggerActivated;
        s.LoggerTimestampsActivated = LoggerTimestampsActivated;
        s.LoggerTimestampFormat = LoggerTimestampFormat;
        s.Save();
    }
}
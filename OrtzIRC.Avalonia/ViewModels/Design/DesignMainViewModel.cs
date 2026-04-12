namespace OrtzIRC.Avalonia.ViewModels.Design;

using CommunityToolkit.Mvvm.ComponentModel;

public class DesignMainViewModel : ObservableObject
{
    public MTObservableCollection<IrcViewModel> Panels { get; }
    public IrcViewModel SelectedPanel { get; set; }

    public DesignMainViewModel()
    {
        var server = new ServerViewModel();
        Panels = new MTObservableCollection<IrcViewModel> { server };
        SelectedPanel = server;
    }
}

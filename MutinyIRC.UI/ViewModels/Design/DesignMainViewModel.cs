using CommunityToolkit.Mvvm.ComponentModel;

namespace MutinyIRC.UI.ViewModels.Design;

public class DesignMainViewModel : ObservableObject
{
    public MTObservableCollection<IrcViewModel> Panels { get; }
    public MTObservableCollection<ServerViewModel> Servers { get; }
    public IrcViewModel SelectedPanel { get; set; }

    /// <summary>The connection the sidebar lists panels for. See MainViewModel.ShownServer.</summary>
    public ServerViewModel ShownServer { get; }

    /// <summary>Unread on a connection the sidebar is not showing. See MainViewModel.</summary>
    public bool HasUnreadElsewhere => false;

    public DesignMainViewModel()
    {
        var server = new ServerViewModel();
        Panels = new MTObservableCollection<IrcViewModel> { server };
        Servers = new MTObservableCollection<ServerViewModel> { server };
        SelectedPanel = server;
        ShownServer = server;
    }
}

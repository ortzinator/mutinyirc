namespace MutinyIRC.UI.ViewModels;

using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Common;

public abstract class IrcViewModel : ViewModelBase, IDisposable
{
    public MTObservableCollection<ChatItemViewModel> ChatLines { get; protected set; }

    /// <summary>The server connection this panel belongs to, or null if it has none.</summary>
    public abstract Server? OwningServer { get; }

    private bool _isSelected;
    public virtual bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public IrcViewModel()
    {
        ChatLines = new MTObservableCollection<ChatItemViewModel>();
    }

    /// <summary>
    ///   Appends an incoming private NOTICE from <paramref name="nick"/>. Lives on the base type
    ///   because a private notice is routed to whichever panel is active, not only the server window.
    /// </summary>
    public void AddPrivateNotice(string nick, string message)
        => ChatLines.Add(new PrivateNoticeViewModel(DateTime.Now, message, nick));

    /// <summary>
    ///   Appends an incoming away reply (RPL_AWAY) telling you that <paramref name="nick"/> is
    ///   away. Lives on the base type because it is routed to whichever panel is active, like a
    ///   private notice, rather than only the server window.
    /// </summary>
    public void AddAwayReply(string nick, string message)
        => ChatLines.Add(new ChatItemViewModel(DateTime.Now, $"{nick} is away: {message}"));

    private RelayCommand<string>? executeCommand;
    public ICommand ExecuteCommand
    {
        get { return executeCommand ?? (executeCommand = new RelayCommand<string>(OnExecute)); }
    }

    protected virtual void OnExecute(string? commandLine)
    {
        if (string.IsNullOrEmpty(commandLine))
            return;

        ChatLines.Add(new ChatItemViewModel(DateTime.Now, commandLine));
    }

    public abstract void Dispose();
}

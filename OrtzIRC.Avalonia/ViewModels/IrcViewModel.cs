namespace OrtzIRC.Avalonia.ViewModels;

using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

public abstract class IrcViewModel : ViewModelBase, IDisposable
{
    public MTObservableCollection<ChatItemViewModel> ChatLines { get; protected set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public IrcViewModel()
    {
        ChatLines = new MTObservableCollection<ChatItemViewModel>();
    }

    private RelayCommand<string>? executeCommand;
    public ICommand ExecuteCommand
    {
        get { return executeCommand ?? (executeCommand = new RelayCommand<string>(OnExecute)); }
    }

    protected virtual void OnExecute(string commandLine)
    {
        ChatLines.Add(new ChatItemViewModel(DateTime.Now, commandLine));
    }

    public abstract void Dispose();
}

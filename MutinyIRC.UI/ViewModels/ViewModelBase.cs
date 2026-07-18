using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MutinyIRC.UI.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private RelayCommand? _closeCommand;
    public ICommand CloseCommand
    {
        get { return _closeCommand ?? (_closeCommand = new RelayCommand(Close, CanClose)); }
    }

    public event EventHandler? RequestClose;

    public virtual void Close()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public virtual bool CanClose() => true;
}

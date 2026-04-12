namespace OrtzIRC.Avalonia.ViewModels;

using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public abstract class ViewModelBase : ObservableObject
{
    public string Name { get; set; } = string.Empty;

    private RelayCommand? closeCommand;
    public ICommand CloseCommand
    {
        get { return closeCommand ?? (closeCommand = new RelayCommand(Close, CanClose)); }
    }

    public event EventHandler? RequestClose;

    public virtual void Close()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public virtual bool CanClose() => true;
}

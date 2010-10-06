namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using System.Windows.Input;
    using MvvmFoundation.Wpf;

    public abstract class ViewModelBase : ObservableObject
    {
        public string Name { get; set; }

        private RelayCommand closeCommand;
        public ICommand CloseCommand
        {
            get { return closeCommand ?? (closeCommand = new RelayCommand(Close, CanClose)); }
        }

        public event EventHandler RequestClose;

        public virtual void Close()
        {
            if (RequestClose != null)
            {
                RequestClose(this, EventArgs.Empty);
            }
        }

        public virtual bool CanClose()
        {
            return true;
        }
    }
}

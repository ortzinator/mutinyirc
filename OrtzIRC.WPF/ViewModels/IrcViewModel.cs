namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using System.Windows.Input;
    using MvvmFoundation.Wpf;

    public abstract class IrcViewModel : ViewModelBase, IDisposable
    {
        public MTObservableCollection<ChatItemViewModel> ChatLines { get; protected set; }

        public IrcViewModel()
        {
            ChatLines = new MTObservableCollection<ChatItemViewModel>();
        }

        private RelayCommand<string> executeCommand;
        public ICommand ExecuteCommand
        {
            get
            {
                return executeCommand ?? (executeCommand = new RelayCommand<string>(OnExecute));
            }
        }

        protected virtual void OnExecute(string commandLine)
        {
            ChatLines.Add(new ChatItemViewModel(DateTime.Now, commandLine));
        }

        public abstract void Dispose();
    }
}

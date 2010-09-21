namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using System.Collections.Generic;
    using MvvmFoundation.Wpf;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    public class IrcViewModel : ObservableObject
    {
        public MTObservableCollection<ChatItemViewModel> ChatLines { get; set; }
        public string Name { get; set; }

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
    }
}

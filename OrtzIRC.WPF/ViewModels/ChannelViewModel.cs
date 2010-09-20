namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using System.Collections.Generic;
    using MvvmFoundation.Wpf;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    public class ChannelViewModel : ObservableObject
    {
        public ObservableCollection<ChatItemViewModel> ChatLines { get; set; }

        public ChannelViewModel()
        {
            ChatLines = new ObservableCollection<ChatItemViewModel>();
        }

        private RelayCommand<string> executeCommand;
        public ICommand ExecuteCommand
        {
            get
            {
                return executeCommand ?? (executeCommand = new RelayCommand<string>(OnExecute));
            }
        }

        private void OnExecute(string commandLine)
        {
            ChatLines.Add(new ChatItemViewModel(DateTime.Now, commandLine));
        }
    }
}

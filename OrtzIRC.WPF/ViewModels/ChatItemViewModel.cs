namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using FlamingIRC;
    using MvvmFoundation.Wpf;

    public class ChatItemViewModel : ObservableObject
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }

        public ChatItemViewModel() { }

        public ChatItemViewModel(DateTime time, string message)
        {
            Time = time;
            Message = message;
        }
    }

    public class ChannelMessageChatItemViewModel : ChatItemViewModel
    {
        public User User { get; set; }

        public ChannelMessageChatItemViewModel() { }

        public ChannelMessageChatItemViewModel(DateTime time, string message, User user)
            : base(time, message)
        {
            User = user;
        }
    }
}

namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using FlamingIRC;
    using MvvmFoundation.Wpf;

    public class ChatItemViewModel : ObservableObject
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }

        protected ChatItemViewModel() { }

        public ChatItemViewModel(DateTime time, string message)
        {
            Time = time;
            Message = message;
        }
    }

    public class ChannelMessageViewModel : ChatItemViewModel
    {
        public User User { get; set; }

        protected ChannelMessageViewModel() { }

        public ChannelMessageViewModel(DateTime time, string message, User user)
            : base(time, message)
        {
            User = user;
        }

        public ChannelMessageViewModel(DateTime time, string message, string nick)
            : base(time, message)
        {
            User = new User { Nick = nick };
        }
    }

    public class ChannelActionViewModel : ChannelMessageViewModel
    {
        public ChannelActionViewModel(DateTime time, string message, string nick)
            : base(time, message, nick)
        {
        }
    }

    public class JoinMessageViewModel : ChannelMessageViewModel
    {
        public string Channel { get; private set; }
        public JoinMessageViewModel(DateTime time, string channel, User user)
        {
            Time = time;
            User = user;
            Channel = channel;
        }
    }

    public class IrcErrorViewModel : ChatItemViewModel
    {
        public string Code { get; private set; }
        public IrcErrorViewModel(DateTime time, string message, string code)
            : base(time, message)
        {
            Code = code;
        }
    }

    public class ErrorMessageViewModel : ChatItemViewModel
    {
        public ErrorMessageViewModel(DateTime time, string message)
            : base(time, message)
        {
        }
    }
}

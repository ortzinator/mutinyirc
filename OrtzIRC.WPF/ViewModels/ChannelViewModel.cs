namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using FlamingIRC;
    using OrtzIRC.Common;

    public class ChannelViewModel : IrcViewModel
    {
        public Channel Channel { get; private set; }
        

        public ChannelViewModel(Channel chan)
        {
            Channel = chan;
            Name = chan.Name;
        }

        protected override void OnExecute(string commandLine)
        {
            ChatLines.Add(new ChannelMessageChatItemViewModel(DateTime.Now, commandLine, new User { Nick = "Ortzinator" }));
        }
    }
}

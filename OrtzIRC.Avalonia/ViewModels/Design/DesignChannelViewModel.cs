namespace OrtzIRC.Avalonia.ViewModels.Design;

using System;
using System.Collections.Generic;
using FlamingIRC;

public class DesignChannelViewModel : IrcViewModel
{
    public string ChannelName => "#general";
    public new string Name => "#general (3)";
    public List<UserViewModel> UserList { get; }

    public DesignChannelViewModel()
    {
        UserList = new List<UserViewModel>
        {
            new UserViewModel(User.FromNames("@Alice")),
            new UserViewModel(User.FromNames("+Bob")),
            new UserViewModel(User.FromNames("Charlie")),
        };

        ChatLines.Add(new ChatItemViewModel(DateTime.Now, "*** Joined #general"));
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, "Hey everyone!", "Alice"));
        ChatLines.Add(new ChannelActionViewModel(DateTime.Now, "waves hello", "Bob"));
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, "Welcome to the channel!", "Charlie"));
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, "Thanks!", "Alice"));
    }

    public override void Dispose() { }
}

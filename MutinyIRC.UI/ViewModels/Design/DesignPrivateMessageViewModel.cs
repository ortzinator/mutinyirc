
using System;

namespace MutinyIRC.UI.ViewModels.Design;

public class DesignPrivateMessageViewModel : PrivateMessageViewModel
{
    public DesignPrivateMessageViewModel()
    {
        OtherNick = "Alice";
        Name = "Alice";
        IsServerConnected = true;

        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, "Hey, got a minute?", "Alice"));
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, "Sure, what's up?", "Ortzinator"));
        ChatLines.Add(new ChannelActionViewModel(DateTime.Now, "tilts head", "Alice"));
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, "Did you see the new patch?", "Alice"));
    }

    public override void Dispose() { }
}

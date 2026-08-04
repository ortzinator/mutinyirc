using System;
using System.Collections.Generic;
using FlamingIRC;
using MutinyIRC.Common;

namespace MutinyIRC.UI.ViewModels.Design;

public class DesignChannelViewModel : IrcViewModel
{
    public string ChannelName => "#general";
    public new string Name => "#general (8)";
    public List<UserViewModel> UserList { get; }
    public override Server? OwningServer => null;

    // Enough of each status to show every group heading and prefix glyph in the previewer.
    public IReadOnlyList<IUserListRow> UserRows { get; }
    public string UserFilter { get; set; } = string.Empty;
    public string UserFilterWatermark => $"Filter {UserList.Count} members";

    public DesignChannelViewModel()
    {
        UserList = new List<UserViewModel>
        {
            new UserViewModel(User.FromNames("~founder")),
            new UserViewModel(User.FromNames("@ChanServ"), "bot"),
            new UserViewModel(User.FromNames("@mira")),
            new UserViewModel(User.FromNames("%hexley")),
            new UserViewModel(User.FromNames("+dt_null")),
            new UserViewModel(User.FromNames("+lucaskim")),
            new UserViewModel(User.FromNames("ada_l"), "you"),
            new UserViewModel(User.FromNames("borkbork")),
        };

        UserRows = UserListGrouping.Build(UserList);

        ChatLines.Add(new ChatItemViewModel(DateTime.Now, "*** Joined #general"));
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, "Hey everyone!", "mira"));
        ChatLines.Add(new ChannelActionViewModel(DateTime.Now, "waves hello", "dt_null"));
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, "Welcome to the channel!", "ada_l"));
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, "Thanks!", "borkbork"));
    }

    public override void Dispose() { }
}

using System;
using FlamingIRC;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MutinyIRC.UI.ViewModels;

public class UserViewModel : ObservableObject, IComparable<UserViewModel>
{
    private readonly User _user;

    public string FullNick => _user.NamesLiteral;

    /// <summary>The bare nickname without any mode prefix (used for command targets).</summary>
    public string Nick => _user.Nick;

    private Mode _mode;
    public Mode Mode => _mode;

    public UserViewModel(User user)
    {
        _user = user;
        _mode = user.Prefix switch
        {
            '@' => Mode.Op,
            '+' => Mode.Voice,
            _ => Mode.Regular,
        };
    }

    public int CompareTo(UserViewModel? other) => FullNick.CompareTo(other?.FullNick);

    public override string ToString() => FullNick;
}

public enum Mode
{
    Owner,
    Op,
    Voice,
    Regular
}

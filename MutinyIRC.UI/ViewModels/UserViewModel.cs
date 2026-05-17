namespace MutinyIRC.UI.ViewModels;

using System;
using FlamingIRC;
using CommunityToolkit.Mvvm.ComponentModel;

public class UserViewModel : ObservableObject, IComparable<UserViewModel>
{
    private readonly User user;

    public string FullNick => user.NamesLiteral;

    private Mode mode;
    public Mode Mode => mode;

    public UserViewModel(User user)
    {
        this.user = user;
        mode = user.Prefix switch
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

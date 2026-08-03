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

    private readonly Mode _mode;
    public Mode Mode => _mode;

    /// <summary>
    /// The glyph shown in the user list's prefix column. Users with no status get a dim
    /// middle dot so every row's nick starts on the same x, prefixed or not.
    /// </summary>
    public string PrefixGlyph => _user.Prefix == '\0' ? "·" : _user.Prefix.ToString();

    /// <summary>Rows in the user list are selectable; the group headers between them are not.</summary>
    public bool IsSelectable => true;

    /// <summary>
    /// Optional badge on the right of the row — "you" for the local user, "bot" for a network
    /// service. Null on everyone else, which is what <see cref="HasTag" /> hides the badge on.
    /// </summary>
    public string? Tag { get; }

    public bool HasTag => !string.IsNullOrEmpty(Tag);

    public UserViewModel(User user, string? tag = null)
    {
        Tag = tag;
        _user = user;
        _mode = user.Prefix switch
        {
            '~' or '&' => Mode.Owner,
            '@' => Mode.Op,
            '%' => Mode.HalfOp,
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
    HalfOp,
    Voice,
    Regular
}

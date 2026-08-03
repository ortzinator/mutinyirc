namespace MutinyIRC.UI.ViewModels;

/// <summary>
/// A section heading in the user list ("OPERATORS 3"). Headers and <see cref="UserViewModel" />
/// rows share one flat list so a single ListBox can render both, which keeps selection and the
/// right-click menu working across the whole list. <see cref="IsSelectable" /> is what stops a
/// header from ever becoming the selected user.
/// </summary>
public sealed class UserGroupHeaderViewModel
{
    /// <summary>Group label, upper-cased for display.</summary>
    public string Name { get; }

    /// <summary>How many users the group shows — after filtering, so it tracks what you see.</summary>
    public int Count { get; }

    public bool IsSelectable => false;

    public UserGroupHeaderViewModel(string name, int count)
    {
        Name = name.ToUpperInvariant();
        Count = count;
    }

    public override string ToString() => $"{Name} {Count}";
}

namespace MutinyIRC.UI.ViewModels;

/// <summary>
/// One row in a channel's user list. The list is flat and holds two unrelated kinds — the
/// <see cref="UserGroupHeaderViewModel" /> headings and the <see cref="UserViewModel" /> rows
/// beneath them — so a single ListBox can own selection and the right-click menu across all of it.
/// This is the only thing the two kinds have in common, and the only thing the container theme
/// binds to; everything else comes from the per-type DataTemplate.
/// </summary>
public interface IUserListRow
{
    /// <summary>
    /// Whether the row can take the pointer, keyboard focus, and the selection. False on headings,
    /// which makes them inert: <c>ChannelView.axaml</c> binds both <c>IsHitTestVisible</c> and
    /// <c>Focusable</c> to it, so neither a click nor an arrow key can land on one.
    /// </summary>
    bool IsSelectable { get; }
}

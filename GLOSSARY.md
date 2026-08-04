# Glossary

Project-specific terminology used in code and discussion. Add a term here when its name is ambiguous or non-obvious from the code alone.

## Ban mask

The `nick!user@host` pattern a channel ban (mode `+b`) matches against; any component may be a `*`
wildcard, so `*!*@dsl.example.net` bans a whole host. `Channel.ResolveBanMask` turns a plain nick
into one of these by looking the user up in the channel's `UserList`, defaulting to `*!*@host`.

## Panel

The content view for a single server or channel — what fills the main area when an entry is clicked in the sidebar. Modeled in code as `IrcViewModel` (base class), with `ServerViewModel` and `ChannelViewModel` as the concrete kinds. `MainViewModel` owns the `Panels` collection and tracks `SelectedPanel`.

## Row list (user list)

The flat `IReadOnlyList<IUserListRow>` a channel's user list binds to (`ChannelViewModel.UserRows`),
built by `UserListGrouping.Build`. It interleaves `UserGroupHeaderViewModel` section headings with
the `UserViewModel` rows under each; `IUserListRow` is the contract those two kinds share, and its
sole member `IsSelectable` is the only thing the item container binds to — the rest comes from a
per-type `DataTemplate`. One flat list rather than nested collections is what lets a single
`ListBox` own selection and the right-click menu across the whole list; headings stay inert by
reporting `IsSelectable == false`, which `ChannelView.axaml` binds to both `IsHitTestVisible` (so
the pointer skips them) and `Focusable` (so keyboard navigation does too).

## Sidebar

The fixed-width navigation column on the left edge of `MainWindow`, listing servers and their channels. Clicking an entry sets `SelectedPanel` and swaps the main content area. It draws on `SurfaceNav` and its rows use the `SidebarNavTheme` control theme defined in `MainWindow.axaml`.
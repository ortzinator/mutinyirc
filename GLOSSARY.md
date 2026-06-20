# Glossary

Project-specific terminology used in code and discussion. Add a term here when its name is ambiguous or non-obvious from the code alone.

## Ban mask

The `nick!user@host` pattern a channel ban (mode `+b`) matches against; any component may be a `*`
wildcard, so `*!*@dsl.example.net` bans a whole host. `Channel.ResolveBanMask` turns a plain nick
into one of these by looking the user up in the channel's `UserList`, defaulting to `*!*@host`.

## Panel

The content view for a single server or channel — what fills the main area when an entry is clicked in the sidebar. Modeled in code as `IrcViewModel` (base class), with `ServerViewModel` and `ChannelViewModel` as the concrete kinds. `MainViewModel` owns the `Panels` collection and tracks `SelectedPanel`.

## Sidebar

The fixed-width navigation column on the left edge of `MainWindow`, listing servers and their channels. Clicking an entry sets `SelectedPanel` and swaps the main content area. Themed via the `Sidebar*` dynamic resources (`SidebarBackground`, `SidebarForeground`, `SidebarNavTheme`, etc.).
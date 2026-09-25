# Glossary

Project-specific terminology used in code and discussion. Add a term here when its name is ambiguous or non-obvious from the code alone.

## Ban mask

The `nick!user@host` pattern a channel ban (mode `+b`) matches against; any component may be a `*`
wildcard, so `*!*@dsl.example.net` bans a whole host. `Channel.ResolveBanMask` turns a plain nick
into one of these by looking the user up in the channel's `UserList`, defaulting to `*!*@host`.

## Channel mode syntax

Which channel modes take a parameter on one server, and when. The server announces this in the
CHANMODES and PREFIX tokens of its `005` (ISUPPORT) reply, so the same letter can differ between
networks: `+q` takes a mask on Libera and a nick on Unreal. `FlamingIRC.ChannelModeSyntax` reads these
tokens from the `ServerProperties` that `Listener` holds, and `ChannelModeInfo.ParseModes` uses the
result to give each parameter to its mode. Before the `005` arrives, the RFC 2812 modes apply.

## Connection

One live socket to one [entry point](#entry-point), plus the session state that rides on it: registration,
the nick you actually got, and the channels you are in. Modeled by `MutinyIRC.Common.Server`, whose name
predates the network model and does not mean the same thing as `ServerSettings`. A connection is a
replaceable part inside a [network](#network), not something you name or save: when one drops and the
network dials a different entry point, the network, its channels, and their panels stay. `ServerManager`
holds the live ones — and still calls a `Server` parameter `ntw` in `Remove`, a fossil of the same drift.

## Entry point

One host you may dial to reach a [network](#network): a hostname, the one port to dial it on, and a TLS
flag. Saved as `ServerSettings`. Its place in `NetworkSettings.Servers` is its place in the
[failover order](#failover-order), so the entry points of a network are ranked rather than
interchangeable. A hostname belongs to at most one network. That is what makes
`IrcSettingsManager.GetNetwork(Server)` able to scan every network's hosts for a URL match and stop at the
first hit. "Server" in the settings window means entry point; "server" in `MutinyIRC.Common` means
connection.

## Failover order

The order a [network](#network) dials its [entry points](#entry-point) in — top to bottom until one
answers. It is simply the order of `NetworkSettings.Servers`, which the Networks dialog lets you rearrange.
`NetworkSettings.ToConnectionCandidates` turns it into the list a `Server` walks: a failed dial steps to
the next host at once, and only a pass that runs off the end starts a round of the reconnect backoff. Thus
"retry N" in the sidebar counts passes over the hosts, not hosts.

## Network

The thing you connect to — "Libera", not `irc.libera.chat`. A network owns its entry points, its channels
(which is why the autojoin list hangs off `NetworkSettings` and not off a host), and at most one live
[connection](#connection) at a time. Saved as `NetworkSettings`; `IrcSettingsManager` holds them all. Every
connection belongs to exactly one network, so dialing a host that no saved network lists mints a network
for it.

A network is identified by its set of entry points, not by its name. The name is whatever the server
reports in the `NETWORK` token of its `005` reply (`Connection.ServerProperties["Network"]`), so it can
change on any connect and two networks may briefly carry the same one. `NetworkSettings` therefore has no
value equality: one instance is one network, and you find a network by entry point with
`IrcSettingsManager.GetNetwork(Server)`. The name is only how a host that reports a known name finds the
network it belongs to — see `NetworkSettingsList.GetOrAddNetwork`.

No runtime type represents a network yet. A `Server` panel stands in for one, and `ServerViewModel.DoRegister`
writes the network back to settings as a side effect of registering, resolving it through
`ServerViewModel.ResolveNetwork`.

## Panel

The content view for one place you can read and talk — what fills the main area when an entry is clicked in
the [sidebar](#sidebar). Modeled as `IrcViewModel`, with three concrete kinds: `ServerViewModel` (the
console for a [connection](#connection)), `ChannelViewModel`, and `PrivateMessageViewModel`. What makes a
panel a panel is what `IrcViewModel` supplies to all three: a `ChatLines` collection, an `OwningServer`,
`CompletionCandidates` for Tab completion in the input box, and `IsSelected` and `HasUnread` for how the
sidebar draws its entry.

`MainViewModel` owns `Panels`, a flat collection of every open panel, and tracks `SelectedPanel`. The
sidebar does not bind to that collection: it binds to the [shown connection](#shown-connection) and lists
that connection's `Channels` and `PrivateMessages`, so a channel or private-message panel is held in two
places at once — flat for selection, per-connection for display.

## Row list (user list)

The flat `IReadOnlyList<IUserListRow>` a channel's user list binds to (`ChannelViewModel.UserRows`),
built by `UserListGrouping.Build`. It interleaves `UserGroupHeaderViewModel` section headings with
the `UserViewModel` rows under each; `IUserListRow` is the contract those two kinds share, and its
sole member `IsSelectable` is the only thing the item container binds to — the rest comes from a
per-type `DataTemplate`. One flat list rather than nested collections is what lets a single
`ListBox` own selection and the right-click menu across the whole list; headings stay inert by
reporting `IsSelectable == false`, which `ChannelView.axaml` binds to both `IsHitTestVisible` (so
the pointer skips them) and `Focusable` (so keyboard navigation does too). The list binds its
selection to `SelectedRow`, typed `IUserListRow?` so it can hold either kind; `SelectedUser` is
derived from it with a type test, which is what keeps a heading from ever reaching the context
menu's Kick and Ban.

## Shown connection

The one [connection](#connection) whose panels the [sidebar](#sidebar) lists: `MainViewModel.ShownServer`,
mirrored on each panel as `ServerViewModel.IsShown`. It is always the connection the selected panel belongs
to, so selecting anything switches it, and `ShowServerCommand` switches it directly — returning to the panel
that connection was last showing, or to its console on a first visit. Every other connection lives behind
the **network switcher**, the button at the top of the sidebar that names the shown connection's
[network](#network), reports its `ConnectionStatus`, and drops down the rest.
`MainViewModel.HasUnreadElsewhere` is what marks that button when a connection you cannot see has unread
activity.

## Sidebar

`SidebarView`, the navigation column down the left edge of `MainWindow`: a 48px strip at the top that the
platform treats as the window's title bar and that holds the settings button, the
[network switcher](#shown-connection) under it, and then the panel list for the
[shown connection](#shown-connection) — its console pinned at the top, then its channels and its private
messages under their group headings. Clicking any row runs `SelectPanelCommand`, which sets `SelectedPanel`
and swaps the main content area.

The column starts at 220px and you may drag it between 150 and 500 with the `GridSplitter` that sits in the
next grid column. It draws on `SurfaceNav`.

## Sidebar nav row

Every clickable row in the [sidebar](#sidebar) is one `Button` wearing the `SidebarNavTheme` control theme
(`Themes/SidebarNavTheme.axaml`). The theme's template owns the parts a row may show: `PART_Pill`, the accent
bar that marks the selected row, and `PART_Close`, the close button that appears while the pointer is on the
row. A row opts into the close button with the `closable` class, which also reserves the room for it; a row
without the class never shows one. `PART_Close` runs the panel's `CloseCommand` and names its cost through
`CloseTooltip`, so closing a channel says it parts the channel.
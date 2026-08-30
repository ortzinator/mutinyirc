# Sidebar migration — network switcher and panel list

Migrates two regions of `design/Mutiny IRC Client.dc.html` into `MutinyIRC.UI`: the network switcher at the
top of the sidebar, and the panel list under it. The title-bar strip, the user footer, the content area and
every dialog stay out of scope.

The mockup renders one network at a time. That single decision is what the rest of this spec hangs off:
`MutinyIRC.UI` gains a *shown connection*, the sidebar draws only that connection's panels, and every other
connection lives behind the switcher.

## Inventory

Source of truth is the `renderVals()` block or CSS variable each element comes from.

| # | Item | Verdict | Target symbol | Mockup source |
|---|---|---|---|---|
| 1 | Switcher button shell | Migrate | `MainWindow.axaml`, the stock Fluent `Button` | `netBtnStyle` |
| 2 | Network chip (colour + monogram) | **Cut** | — | `railChipStyle`, `nets[].mono` |
| 3 | Network name | Adapt | `ServerViewModel.NetworkName` | `railName` |
| 4 | Connection state line | Adapt | `ServerViewModel.ConnectionStatus` | `railState`, `railStateStyle` |
| 5 | Aggregate unread badge | Adapt → dot | `MainViewModel.HasUnreadElsewhere` | `railBadge` |
| 6 | Chevron | Migrate | `ic:SymbolIcon Symbol="ChevronDown"` | inline `<svg>` |
| 7 | Dropdown surface | Adapt → `Flyout` | `MainWindow.axaml` | `netMenu` block |
| 8 | Dropdown network row | Adapt | `MainViewModel.Servers` | `networks[]` |
| 9 | "Manage networks…" | Migrate | `ServersMenuItem_Click` → `ServerSettingsWindow` | `openConnect` |
| 10 | One network at a time | Migrate | `MainViewModel.ShownServer` | `byNet[rail]` |
| 11 | Group headings | Adapt | `MainWindow.axaml` | `groups[].name` |
| 12 | Group heading meta | **Cut** | — | `groups[].meta` |
| 13 | Row shell | Migrate | `SidebarNavTheme` | `mk().rowStyle` |
| 14 | Selection pill | Migrate | `SidebarNavTheme`, `AccentFillBrush` | `mk().pill` |
| 15 | Glyph column | Adapt | `ChannelViewModel.Prefix`; `Server` and `Person` glyphs | `mk().icon`, `iconStyle` |
| 16 | DM presence dot | **Cut** | — | `mk().iconStyle` (`o.away`) |
| 17 | Label weight and colour | Migrate | `SidebarNavTheme` (`.unread`, `.active`) | `mk().labelStyle` |
| 18 | Lock glyph on keyed channels | Migrate | `Channel.Key`, `ChannelViewModel.IsKeyed` | `mk().locked` |
| 19 | Per-row unread badge | **Cut** | — | `mk().badgeStyle` |
| 20 | Server console row | Adapt — pinned row | `MainWindow.axaml` | absent from the mockup |

## Conflicts

**The sidebar's shape.** The mockup shows one network; `MainWindow.axaml` binds an `ItemsControl` to
`MainViewModel.Servers` and nests every connection's channels and private messages beneath it.
*Resolution:* adopt the mockup. `MainViewModel` gets `ShownServer`, independent of `SelectedPanel`, and the
panel list binds through it.

**Switching a network against selecting a panel.** The mockup's `onClick` sets `rail` and `active` together.
This project keeps `SelectedPanel` as the one thing that drives the content area.
*Resolution:* switching restores that connection's last-viewed panel, and falls back to its console on a
first visit. `MainViewModel` remembers the last panel per connection.

**The console has no row in the mockup.** `ServerViewModel` is a real panel with real content — MOTD,
connect and disconnect lines, errors, whois output — and today it is the tree's parent row. The mockup lists
only channels and direct messages, which would make it unreachable.
*Resolution:* a pinned row above the `CHANNELS` heading, labelled with the connection's hostname
(`Server.Url`). The switcher button above it already carries the network name, so the row does not repeat it.

**"Server" means two things.** The mockup calls a network a server; `GLOSSARY.md` reserves *connection* for
`MutinyIRC.Common.Server` and *entry point* for a host you dial.
*Resolution:* the switcher lists live connections, one per `ServerViewModel`, and calls each by its network
name. A saved network with no connection has no panels to switch to and does not appear; "Manage networks…"
is the door to those.

**The `#` glyph lies about `&` channels.** `ChannelViewModel.ChannelName` trims a leading `#`, `&`, `+` or
`!` and the view draws a `NumberSymbol` icon regardless, so `&chan` renders as if it were `#chan`.
*Resolution:* the glyph column shows the channel's real prefix character and the label shows the trimmed
name.

**Closing a channel does not leave it.** `CloseCommand` fires `RequestClose`, and
`MainViewModel.Chan_RequestClose` disposes the panel while the `Channel` survives in `Server.Channels` — you
stay joined with no window. `Channel.Part()` exists in `MutinyIRC.Common` and no UI code calls it.
*Resolution:* close parts the channel when it is still joined, then closes the panel. Both channel and
private-message rows carry the hover-reveal close button.

## Gaps

| Gap | Decision | Layer |
|---|---|---|
| No observable connection state on a panel | Build `ConnectionStatus`, driven by the events `ServerViewModel` already handles | `MutinyIRC.UI` |
| Reconnect attempt number is private (`Server._reconnectAttempt`) | Build — `Reconnecting` carries the attempt beside the delay | `MutinyIRC.Common` |
| No SASL, no CAP negotiation anywhere in FlamingIRC | **Cut** the `· SASL` segment | — |
| `Channel.Key` is never written; nothing folds `+k` back into the channel | Build — handle `+k`/`-k` and RPL_CHANNELMODEIS | `MutinyIRC.Common` |
| No unread count; `HasUnread` is a bool | **Cut** every numeric badge | — |
| No per-user away state; no `away-notify` | **Cut** the DM presence dot | — |
| No per-network accent or monogram | **Cut** the chip | — |
| No accent *fill* brush in the theme | Build `AccentFillBrush` in both variants, for the selection pill and the unread dot | `MutinyIRC.UI` |
| Nothing tracks unread on a hidden connection | Build `MainViewModel.HasUnreadElsewhere` and `ServerViewModel.HasUnreadAnywhere` | `MutinyIRC.UI` |

## Behaviour

- **Switching** sets `ShownServer`, then selects that connection's last-viewed panel, or its console.
- **A new connection** takes over the sidebar and selects its console. This replaces the old rule in
  `CreateServerPanel`, which selected a panel only when nothing was selected.
- **No connections** leaves the button reading "Not connected" with no state line; its dropdown offers only
  "Manage networks…", and the panel list is empty.
- **Unread elsewhere** puts an accent dot on the switcher button, and on each dropdown row whose connection
  has unread.
- **Connection status** reads `Connected · TLS`, `Connected`, `Connecting…`, `Reconnecting · retry N` or
  `Disconnected`. TLS comes from `Connection.ConnectionData.Ssl`.

## Notes from the build

- The switcher's shell needs no control theme of its own. The stock Fluent `Button` already draws
  `ButtonBackground`, its hover and `ButtonBorderBrush`, which are the three values `netBtnStyle` asks
  for; the accent outline on the open menu is one setter on `:flyout-open`.
- The glyph column carries the channel's prefix, as decided, and a glyph on the two row kinds that have
  no prefix: `Server` on the console and `Person` on a direct message. Neither claims any state — the
  presence dot stays cut.
- `Channel.Key` is null when a channel has no key, so the padlock binds to a channel that is keyed, not
  to a key we know. A server that hides the key from members still gets the padlock.

## Cut, and why

- **Every numeric badge** (items 5, 19, and the badge in 8). The codebase has no unread count and no
  mention detection. Bold marks activity; a dot marks activity you cannot see. Adding a count is a separate
  piece of work with its own question — what counts as a mention.
- **The DM presence dot** (16). The only away signal we get is RPL_AWAY, which arrives after you message
  someone. A dot that reads "present" because we never learned otherwise is worse than no dot. `away-notify`
  needs IRCv3 CAP negotiation in FlamingIRC and per-user state in `MutinyIRC.Common` — its own run.
- **SASL** (part of 4). FlamingIRC has no capability negotiation at all.
- **The network chip** (2). Its two colours are not in the theme, and the palette's ten hues are tuned as
  text colours, so a chip would need a per-variant foreground of its own. The name and state line identify a
  network without it.
- **The group heading meta** (12). It repeated the connection status that the button already shows.
- **Collapsible groups** (part of 11). The mockup draws the chevron and wires it to nothing.
- **Sidebar width.** Stays 220px with its `GridSplitter`. The mockup's 284px is proportional to a 1440px
  window; this app opens at 1100px, and the drag range already reaches 284.
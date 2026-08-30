# Network settings migration — the Networks dialog

Migrates the connection-manager dialog of `design/Mutiny IRC Client.dc.html` (markup 375–471, logic in
`renderVals()` 816–880) into `MutinyIRC.UI/Views/ServerSettingsWindow.axaml`. The settings dialog beside
it, the main window and the title bar stay out of scope.

The mockup is not a reskin of today's window. Its server table gives each row one port, an order number, a
drag grip and the hint "Tried top to bottom until one answers". That is **ordered failover**, and it is the
first UI for `docs/adr/0001-network-as-runtime-aggregate.md`. Adopting it moves three facts off the entry
point and onto the network — the port becomes singular, the nick and the autoconnect flag belong to the
network — and it fixes a live bug on the way: `ConnectionArgs`'s only constructor hardcodes `Port = 6667`
(`FlamingIRC/ConnectionArgs.cs:51`), so the port a user saved was edited, persisted, and then dropped at
dial time.

Intended outcome: a network is edited as one thing — its ordered hosts, its identity, its autojoin
channels — and dialling walks those hosts in order.

## Inventory

Source of truth is the `renderVals()` block or the markup line each element comes from.

| # | Item | Verdict | Target symbol | Mockup source |
|---|---|---|---|---|
| 1 | Dialog shell (scrim, shadow, `irisIn`) | Adapt → OS `Window` | `ServerSettingsWindow.axaml` | 372–376 |
| 2 | Title + subtitle | Migrate | header `Border` | 378–379 |
| 3 | Network rail card (228px) | Migrate | left `Grid` column | 382–400 |
| 4 | Network row status dot | **Cut** | — | `netList[].dot` |
| 5 | Network row name | Migrate | `NetworkSettingsViewModel.Name` | `netList[].name` |
| 6 | Network row live state text | **Cut** | — | `netList[].state` |
| 7 | `+` / `−` strip on the rail | Migrate | `AddNetworkCommand`, `RemoveNetworkCommand` | 392–399 |
| 8 | Servers table + "tried top to bottom" hint | Migrate | `ItemsControl` in the right pane | 403–429 |
| 9 | Ordered failover itself | **Build** | `Server` candidate walk | that hint |
| 10 | Drag grip / reorder | Adapt → `▲`/`▼` buttons | `MoveServerUpCommand` / `MoveServerDownCommand` | `s.gripStyle` |
| 11 | Order number | Migrate | `ServerSettingsViewModel.Order` | `s.order` |
| 12 | Host | Migrate | `ServerSettingsViewModel.Url` | `s.host` |
| 13 | Port, one per row | **Build** — schema change | `ServerSettings.Port` (`int`) | `s.port` |
| 14 | TLS badge | Migrate | `ServerSettingsViewModel.Ssl` | `s.tlsStyle` |
| 15 | Server row live state | **Cut** | — | `s.stateStyle` |
| 16 | Row `×` remove | Migrate | `RemoveServerCommand(row)` | `s.onRemove` |
| 17 | Inline `add server — host:port` | Migrate | `NewServerText` + `AddServerCommand` | 425–428, `parseServer()` |
| 18 | Toggle — Verify TLS certificates | Adapt → drawn, disabled | — | `connToggles[0]` |
| 19 | Toggle — SASL authentication | Adapt → drawn, disabled | — | `connToggles[1]` |
| 20 | Toggle — Connect on startup | Adapt → drawn, disabled; flag moves | `NetworkSettings.AutoConnect` | `connToggles[2]` |
| 21 | Toggle control (switch, not checkbox) | Migrate | stock `ToggleSwitch` | `toggle(on)` |
| 22 | Nickname field | **Build** | `NetworkSettings.Nick` | 443–446 |
| 23 | Alternate field | **Build** | `NetworkSettings.AlternateNick` | 447–450 |
| 24 | Autojoin chips + "+ Add channel" | Migrate | `NetworkSettingsViewModel.Channels` | 453–464 |
| 25 | Footer `Connect` / `Cancel` | Adapt → `Save` / `Cancel` | existing click handlers | 467–470 |
| 26 | Full-width split footer | **Cut** | — | 467–470 |
| 27 | Channel key field | Adapt → rides the add input | `ChannelSettings.Key` | absent from the mockup |
| 28 | Server `Description` field | **Cut** | — | absent from the mockup |

Network rename is absent from the mockup but kept. `BeginEdit` / `CommitEdit` / `CancelEdit` on
`NetworkSettingsViewModel` already work, and a network you have never connected to has no other handle.
`ServerViewModel.ResolveNetwork` writes `Name` from the server's `005` `NETWORK` token on every
registration, so a rename only survives on a network you have not connected to.

## Conflicts

**A set of candidate ports against one port per row.** `ServerSettings.Ports` is a string of comma-and-range
port candidates, and `RandomPort` picks one. The mockup gives each host exactly one port and orders the
hosts instead.
*Resolution:* adopt the mockup. `Ports` becomes `int Port`. `RandomPort`, `PortList` and
`NetworkSettings.GetRandomServer` are deleted — all three were dead in production and reached only by tests,
because `MainViewModel.Start` never read a port at all. `Load()` folds a legacy `ports` string down to its
first value.

**Autoconnect on the entry point against autoconnect on the network.** Today every entry point carries its
own `AutoConnect`, and `Start()` dials each flagged host, so one network with three hosts means three
connections. ADR 0001 says a network holds at most one connection.
*Resolution:* the flag moves to `NetworkSettings`. `Load()` folds the old flags up — a network autoconnects
if any host it listed did — and `GetAutoConnectServers()` becomes `GetAutoConnectNetworks()`.

**Nick on the entry point against nick on the network.** `ServerSettings.Nick` was per host, so failing over
between two hosts of one network could change your identity.
*Resolution:* `NetworkSettings.Nick` and `AlternateNick`, both blank-falls-back to `AppSettings.FirstNick` /
`SecondNick`. `Load()` folds the first non-null entry-point nick up into the network.

**Selection-driven detail forms against a table.** Today's window is master/detail three times over:
select a network, select an entry point, edit its fields in a form. The mockup edits every row in place.
*Resolution:* `SelectedServer` and `SelectedChannel` go with the forms they gated. Remove and reorder take
the row as a command parameter.

**A modal `div` against a window.** The mockup draws its own scrim, shadow and iris animation.
*Resolution:* the dialog stays an OS `Window` shown with `ShowDialog(owner)`. This project has no
FluentAvalonia, so there is no `ContentDialog` to host an in-window modal, and a hand-rolled overlay would
be a second dialog mechanism for one screen.

**`Connect` in the footer against a settings editor.** The mockup's primary button dials.
*Resolution:* `Save` / `Cancel`. The dialog opens from a gear menu and from "Manage networks…", neither of
which promises a connection, and dialling from here would need a network to already have a connection to
replace. Dialling stays with `/server` and with autoconnect.

## Gaps

| Gap | Layer that takes it |
|---|---|
| Ordered failover | `MutinyIRC.Common.Server` — a candidate list and an index, walked on `ConnectFailed` |
| One port per entry point | `MutinyIRC.UI/ServerSettings.cs`, plus a `Load()` migration |
| Network-level identity | `MutinyIRC.UI/NetworkSettings.cs` |
| Reading the saved port at dial time | `NetworkSettings.ToConnectionCandidates()` — the bug fix |
| `host:port` parsing | `NetworkSettingsViewModel`, ported from `parseServer()` |
| `SurfaceCard`, `StatusSuccessForeground` | `Themes/DefaultTheme.axaml`, both variants |
| Design-time data for the window | `MutinyIRC.UI/ViewModels/Design` |

## Behaviour

- **Failover.** A network dials its entry points in list order. A failed connect advances to the next host
  at once, with no delay. Only when the walk passes the last host does the reconnect attempt count rise and
  the existing exponential backoff apply, so one backoff round is one full pass over the hosts and
  `Reconnecting · retry N` still counts passes. Registration ends the run and resets the count.
  The index is not reset with it: it always names the entry point the live connection came from, so
  a dropped link retries the host you were on before the walk moves off it.
- **Adding a server** parses `host` or `host:port`. The default port is 6697 and TLS is inferred as
  `port != 6667 && port != 6665`, both from `parseServer()`. Input that does not parse leaves the Add
  button disabled.
- **Adding a channel** parses `#channel` or `#channel key`. Membership of the list is the autojoin flag, so
  `ToModel()` always writes `AutoJoin = true` and removing the chip is how you stop autojoining.
- **The three toggles are drawn and disabled.** They mark where the settings will live and read the state
  they would control, but none of them writes. See the regression note below.
- **Save** replaces the whole saved set through `NetworkSettingsList.ReplaceAll`, which keeps the
  one-network-per-hostname rule. **Cancel** discards.

### Known regression

With all three toggles disabled, nothing in this dialog can change autoconnect. Autoconnect keeps working
from whatever `servers.json` already holds — the `Load()` fold preserves those values, and connecting to a
host still sets the network's flag in `ServerViewModel.DoRegister` — but the "Auto-connect" checkbox that
the old window offered goes away without a working replacement. This was raised and confirmed as a
deliberate deferral; wiring the three toggles is a later run.

## Cut, and why

- **Every live-state signal** (items 4, 6, 15). The dialog edits saved configuration; a network you have
  never dialled has no state to show, and a network you are connected to already shows its state in the
  sidebar switcher. Worse, the mockup's per-server `connected` / `no route` implies the dialog knows which
  entry point a live connection came through, which would make the editor depend on the runtime graph.
- **The full-width split footer** (26). Two equally weighted, equally sized buttons say the destructive one
  is as expected as the safe one. Right-aligned `Save` / `Cancel` matches every other dialog in the app.
- **`ServerSettings.Description`** (28). The mockup has no field for it, nothing ever displayed it, and the
  network's name is the label a user reads.
- **Drag-and-drop reorder** (part of 10). Order matters now, so reordering must exist, but drag-and-drop is
  its own piece of work — a drop indicator, an adorner, keyboard parity. `▲` / `▼` buttons give the whole
  capability, and are reachable from the keyboard as the grip would not have been.
- **The mockup's fonts.** It asks for Segoe UI Variable Display for the title and Cascadia Code for the
  host and port. The app ships Inter and bundles no monospace, so both fall back to the app font. Tabular
  figures on the port column would need a font that has them.

## Relationship to ADR 0001

`docs/adr/0001-network-as-runtime-aggregate.md` is `status: proposed`. Network-level autoconnect and the
failover walk are its first implementation: one network yields one connection, and moving between hosts is
internal to the network rather than a second connection. The ADR keeps its status here — flipping it is a
decision about the whole model, not a side effect of one screen.

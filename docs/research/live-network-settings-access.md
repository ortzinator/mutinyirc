# Live reads and writes of network settings

Research for [#20](https://github.com/ortzinator/mutinyirc/issues/20), part of map
[#19](https://github.com/ortzinator/mutinyirc/issues/19). All line numbers are at commit `84c5df2`
(branch `avalonia-rewrite`). Sources are the code itself; the Avalonia modal behaviour is from the
Avalonia docs (`docs/app-development/window-management.md`: "The parent window is disabled while the
dialog is open").

"Edited" below means a dialog save changed the **host** of the entry point that the live connection
is on. "Moved" means a dialog save put that entry point (same host) in a different network. A
change of only the port or TLS flag is listed separately where it differs.

## Background facts that every row depends on

1. **The dialog edits copies.** `SettingsViewModel(IrcSettingsManager)`
   (`MutinyIRC.UI/ViewModels/SettingsViewModel.cs:24-30`) builds a `NetworkSettingsViewModel` per
   network, which copies every field (`NetworkSettingsViewModel.cs:83-96`,
   `ServerSettingsViewModel.cs:34-40`). Nothing the app writes to `Networks` after the dialog opens
   reaches the dialog.
2. **A save replaces every object.** `SettingsViewModel.Save` (`SettingsViewModel.cs:56-62`) calls
   `Networks.ReplaceAll(Networks.Select(vm => vm.ToModel()))`, then `IrcSettingsManager.Save()`.
   `ToModel()` (`NetworkSettingsViewModel.cs:258-274`, `ServerSettingsViewModel.cs:86`) makes **new**
   `NetworkSettings` and `ServerSettings` instances. After any OK click, no `NetworkSettings` or
   `ServerSettings` reference held from before the save is in `Networks` any more, even when the user
   changed nothing. `ReplaceAll` (`NetworkSettingsList.cs:86-102`) also drops an entry point whose host
   an earlier network already claims.
3. **The dialog is modal on the main window.** `SidebarView.ManageNetworks_Click`
   (`MutinyIRC.UI/Views/SidebarView.axaml.cs:36-44`) does `await new ServerSettingsWindow().ShowDialog(owner)`.
   The main window is disabled, so no main-window click handler can run while it is open. The UI
   dispatcher keeps running, so `Dispatcher.UIThread.Invoke`/`Post` work from other threads still
   runs. Socket and timer threads are not affected at all.
4. **A live connection has its own copy of the dial list.** `MainViewModel.Start`
   (`MutinyIRC.UI/ViewModels/MainViewModel.cs:158-168`) calls `network.ToConnectionCandidates()`
   (`NetworkSettings.cs:114-129`), which builds new `ConnectionArgs` objects, and passes them to
   `ServerManager.Create`. `Server` copies them into `_candidates` (`MutinyIRC.Common/Server.cs:49,
   102-110`). No code writes `_candidates` from settings after that. Only `ChangeServer`
   (`Server.cs:1207-1230`, from `/server`) replaces it.
5. **`Server.Url` is the host of the current candidate.** `Server.Url => Connection.ConnectionData.Hostname`
   (`Server.cs:122`). It changes when the failover walk moves (`UseCandidate`, `Server.cs:920`).
6. **Host lookups ignore the network and the port.** `IrcSettingsManager.GetServer(string host)`
   (`IrcSettingsManager.cs:263-266`) returns the first `ServerSettings` in any network whose `Url`
   matches the host (case-insensitive). `GetNetwork(Server)` (`:157`) is `GetServer(server.Url)?.Network`.
   `GetEntryPoint(host, port)` (`:255-259`) also matches the port.
7. **IRC events arrive on thread-pool threads.** `TcpTextClient.Connect` starts the session with
   `Task.Run(() => RunSession(...))` (`FlamingIRC/TcpTextClient.cs:138`); the read loop uses
   `ConfigureAwait(false)` (`:203-223`, `:252-276`) and calls `OnReceiveLine`, which runs the
   `Listener` parsers, which raise `Server`'s events inline (`Server.cs:1005-1011`, `:1013-1025`,
   `:985-998`). `ServerViewModel`'s handlers for `Registered`, `NetworkNameReported`, `PartSelf`,
   `NickError` and `ConnectFailed` therefore run on a thread-pool thread, with no marshalling.
   `RunSession`'s remarks (`TcpTextClient.cs:198-202`) say an exception from an event handler is not
   caught and ends the process.
8. **`ServerViewModel._network` caches the resolved network** (`ServerViewModel.cs:605`). `DoRegister`
   sets it (`:588`), `Server_NetworkNameReported` reads and replaces it (`:686-696`), and
   `Server_Connecting` clears it (`:414-419`). `_hostNamedNetwork` (`:610`) is a second cached reference.

## Reads that resolve a live connection to its network or entry point

| # | Where | Thread | Trigger | Runs while the dialog is open? | After **edit** (host) | After **move** |
|---|-------|--------|---------|------|------|------|
| R1 | `MainViewModel.Start` → `GetAutoConnectNetworks` + `ToConnectionCandidates` (`MainViewModel.cs:158-168`, `IrcSettingsManager.cs:144-151`, `NetworkSettings.cs:114`) | UI | App startup, once | No (before any window can open) | n/a: the `Server` keeps the old candidates until the app restarts (fact 4) | n/a: the `Server` keeps the old network's candidates and failover order until restart |
| R2 | `Server.AdvanceOrReconnect` / `UseCandidate` / `Restart` / `DialEntryPoint` (`Server.cs:849`, `:920`, `:656`, `:636`) — reconnect, failover, Reconnect button, Trust redial | Socket thread (failure), timer thread (backoff), UI (buttons) | Dial failure, drop, Reconnect, Trust | Yes for failure/timer paths; no for buttons | Dials the **old** host from `_candidates`; settings are not read | Walks the **old** network's candidate list; settings are not read |
| R3 | `CertificateTrustLookup` → `GetCertificateTrust` (`MainViewModel.cs:155-156`, `IrcSettingsManager.cs:176-187`, called from `Server.AcceptRejectedCertificate` `Server.cs:185-204`) | Thread-pool (TLS handshake) → `Dispatcher.UIThread.Invoke` → UI | Each TLS handshake whose certificate fails the normal check | Yes; it reads the **saved** settings, not the dialog's copy | Old host not in any network → `CertificateTrust.Default` (verify on, no pin). Port-only edit: verify switch still from the network (host match), pin lookup by old port → none | Verify switch comes from the **destination** network; the pin travelled with the entry point (`ToModel` carries it), so the pin still applies |
| R4 | `ServerViewModel.AddRefusal` → `_server.CertificateTrustLookup` (`ServerViewModel.cs:475-496`) | Thread-pool (`ConnectFailed`), lookup marshals to UI | Certificate refused | Yes | Same as R3: no pin found, so the refusal never says "Replace pinned certificate" | Same as R3: pin from the moved entry point |
| R5 | `ServerViewModel.AlternateNick` → `GetNetwork(_server)?.AlternateNick` (`ServerViewModel.cs:318-324`, from `Server_NickError` `:285-311`) | Thread-pool | 433 nick taken before registration | Yes (a reconnect or failover can register while it is open) | `GetNetwork` → null → `AppSettings.SecondNick`. The first nick still comes from the candidate snapshot (R1) | Destination network's alternate nick. The first nick is still the old network's (snapshot), so the two can come from different networks |
| R6 | `ServerViewModel.Server_PartSelf` → `GetNetwork(_server)` (`ServerViewModel.cs:274-283`) | Thread-pool | You part a channel | Yes | `GetNetwork` → null → nothing removed | Looks the channel up in the **destination** network's list and removes it there if the name matches. See W5 |
| R7 | `ServerViewModel.ResolveNetwork` → `GetNetwork(_server)` (`ServerViewModel.cs:650-662`, from `DoRegister` `:579-601`, from `Server_Registered` `:547-555`) | Thread-pool | Every 001 registration, including each reconnect | Yes | `GetNetwork` → null → `GetOrAddNetwork(reported name or host)`. See W1 | Returns the destination network; `DoRegister` then adopts it. See W1, W3 |
| R8 | `ServerViewModel.Server_NetworkNameReported` (`ServerViewModel.cs:686-696`) | Thread-pool (005 `NETWORK=`, `Server.cs:1013-1025`) | First `NETWORK=` of a session, or a changed one | Yes | Works on the cached `_network`, not a fresh lookup. See W3, W4 | Same: cached `_network` |
| R9 | `ServerViewModel.JoinAutoJoinChannels` (`ServerViewModel.cs:629-636`) | Thread-pool (from R7, R8) | Registration; settle into another network | Yes | Joins the channels of whatever network W1 produced (a new host-named network has none) | Joins the **destination** network's autojoin channels on the next registration; the old network's channels are not joined |
| R10 | `IrcSettingsManager.DisableAutoConnect(Server)` (`IrcSettingsManager.cs:160-165`) | — | **No callers** in any project (grep of the solution) | n/a | Would do nothing (`GetNetwork` → null) | Would clear `AutoConnect` on the destination network |
| R11 | `ServerSettingsWindow` / `SettingsViewModel` constructor (`ServerSettingsWindow.axaml.cs:13`, `SettingsViewModel.cs:24-30`) | UI | Dialog opens | Is the dialog | Snapshot of the saved settings at that moment | Same |

No code in `MutinyIRC.Commands` or `MutinyIRC.Common` reads `IrcSettingsManager` (it lives in
`MutinyIRC.UI`). `/server host` (`MutinyIRC.Commands/ServerCommand.cs:62-80`) builds `ConnectionArgs`
from the typed text and calls `ChangeServer` or `ServerManager.Create`; it reads no settings, but
the next registration runs W1 for that host.

## Writes to `IrcSettingsManager.Networks` or a `NetworkSettings` in it

| # | Where | Thread | Trigger | Runs while the dialog is open? | After **edit** (host) | After **move** |
|---|-------|--------|---------|------|------|------|
| W1 | `DoRegister`: `ResolveNetwork` → `GetOrAddNetwork` (`NetworkSettingsList.cs:36-45`) then `network.AddServer(new ServerSettings(_server.Url, _server.Port, ssl))` (`ServerViewModel.cs:581-586`) | Thread-pool | Every 001 | Yes | The old host matches nothing, so the old host is **put back** into settings: in a network named after the host (new, or an existing one with that name), or in the network named by `NETWORK=` if the server sent it before 001. `_hostNamedNetwork` is set. Then W4 at 005 can move it into the network with the reported name — usually the network the user edited, which then holds **both** the new host and the old host. `GetOrAddNetwork` adds by name and does not check the one-host rule | `AddServer` is a no-op: `ServerSettings.Equals` compares `Url` only (`ServerSettings.cs:79`). Port-only or TLS-only edit: also a no-op, so the live port is not written back |
| W2 | `AdoptNetwork`: `network.AutoConnect = true`, `Name`/`NetworkName` of the view model (`ServerViewModel.cs:615-627`) | Thread-pool | From W1 and W4 | Yes | Sets `AutoConnect` on the network W1 produced | Sets `AutoConnect` on the **destination** network, even if the user had it off |
| W3 | `SettleByReportedName`: `network.Name = reportedName` (`ServerViewModel.cs:673-682`) | Thread-pool | 005 `NETWORK=` (via R7 when it came before 001, or via R8) | Yes | Renames the network W1 produced (or the one W4 moved into) to the reported name | **Renames the destination network** to the name the server reports, each session |
| W4 | `IrcSettingsManager.SettleMintedNetwork` (`IrcSettingsManager.cs:237-253`) from `SettleByReportedName` (`ServerViewModel.cs:675-676`) | Thread-pool (the method's remarks say "Call it on the UI thread") | 005 on a network with `MintedByPin` or the `_hostNamedNetwork` | Yes | Clears `MintedByPin`; if the minted/host-named network has one entry point and another saved network has the reported name: `minted.RemoveServer`, `Networks.Remove(minted)`, `named.AddServer(entryPoint)` (no one-host check) | Only when the moved entry point's destination network is itself minted. `MintedByPin` survives a save (`NetworkSettingsViewModel.cs:66, 90, 266`), so a minted network that received a moved entry point and now has two is not merged |
| W5 | `Server_PartSelf` → `nwSettings.RemoveChannel(chan)` (`ServerViewModel.cs:276-282`) | Thread-pool | You part a channel | Yes | No write (R6) | Removes the channel from the destination network's list |
| W6 | `TrustCertificateAsync` → `PinCertificate` (`IrcSettingsManager.cs:195-204`, saves) or `MintPinnedNetwork` (`:216-225`, `TryAdd` `NetworkSettingsList.cs:51-60`, saves), then `_server.DialEntryPoint` (`ServerViewModel.cs:520-545`) | UI | Trust button on the newest refusal (after an optional modal confirm) | **No**: the button is in the main window, which is disabled; the confirm is itself modal on the main window | `PinCertificate(oldHost, port)` finds nothing → `MintPinnedNetwork` mints a new network named after the old host with the pin (unless another network lists the host on another port → error line). The old host is back in settings; at 005 W4 can move it into the reported-name network. `DialEntryPoint` finds the old host in `_candidates`, so the redial works | `GetEntryPoint` is global by host+port, so it pins the entry point **in its destination network** and saves |
| W7 | `SettingsViewModel.Save` (`SettingsViewModel.cs:56-62`) via `OkButton_Click` (`ServerSettingsWindow.axaml.cs:35-39`) | UI | OK in the dialog | Is the dialog | Replaces all objects (fact 2) | Same |
| W8 | `IrcSettingsManager.Save()` to disk: from W6, W7 and `App` main-window `Closing` (`App.axaml.cs:26-34`) | UI | Pin, dialog OK, app close | Pin: no. Close: the main window is disabled while the dialog is open (whether the OS close box still raises `Closing` was not checked) | Writes whatever is in `Networks`, including W1–W5 results | Same |
| W9 | `Load` / `LoadDefaults` (`IrcSettingsManager.cs:70-103`, `:132-138`) via `ReplaceAll` | Thread of the first `IrcSettingsManager.Instance` access (UI, at startup) | Once | No | n/a | n/a |

W1–W5 are not saved to disk at once. They reach disk at the next W8 (pin, dialog OK, app close).

## Interactions with a dialog that is open, then saved

These follow from the facts above. They are facts about today's code, not proposals.

- **Writes during the dialog are lost on OK.** W1–W5 can run while the dialog is open (reconnect,
  failover, 005, part). They change the live `NetworkSettings` objects, which the dialog copied
  before the change. OK (W7) replaces every object with the dialog's copies, so a donated entry
  point, an `AutoConnect = true`, a rename from `NETWORK=`, a settle (W4) and a parted channel are
  all undone. Cancel keeps them.
- **The cache goes stale on any OK.** After W7, `ServerViewModel._network` and `_hostNamedNetwork`
  refer to objects no longer in `Networks` (fact 2), until the next `Server_Connecting`. A later 005
  in the same session (R8) then renames, flags and joins through the orphan object (W2, W3, R9),
  with no effect on saved settings. If the orphan is minted or host-named, W4 runs `named.AddServer`
  on a **live** network with the orphan's entry point, while the live list may already list that
  host (the one-host rule of `NetworkSettingsList` is bypassed).
- **The live dial list never follows a save.** Edit or move, the `Server` keeps dialling the
  candidates it was built with (R1, R2), including on the Reconnect button and the Trust redial.
  Only an app restart (or `/server`) rebuilds it.
- **Trust lookups follow a save at once.** R3/R4 read the saved settings at each handshake, so the
  next handshake after OK uses the new verify switch and pins. For an edited host the old host gets
  the default trust (verify on, no pin) while it is still being dialled.

## Observations (not part of the trace)

- **Observation:** the remarks on `GetCertificateTrust` (`IrcSettingsManager.cs:173-175`) and the
  comment in `MainViewModel.Start` (`:153-154`) say only the UI thread changes the settings. W1–W5
  and R5–R8 run on thread-pool threads (fact 7), and `SettleMintedNetwork`'s remark asks for the UI
  thread. `NetworkSettingsList` and `NetworkSettings.Servers` are plain `List<T>`. A `ReplaceAll` on
  the UI thread while a socket thread enumerates `Networks` in `GetServer` could throw
  "Collection was modified", which `RunSession` does not catch (fact 7).
- **Observation:** after a host edit, the first registration of the still-running connection puts
  the old host back into settings (W1, and W6 for a Trust), and a later `NETWORK=` can put it back
  into the edited network (W4). The edit is then partly undone the next time settings are saved.
- **Observation:** after a move, every registration renames the destination network to the server's
  reported name (W3) and turns its `AutoConnect` on (W2).

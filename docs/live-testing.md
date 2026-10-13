# Live testing

How to check a change in the running app: screenshots, UI Automation through FlaUI, and tests against a real IRC
server. Start the app with its log written to a file, as `CLAUDE.md` "Runtime Error Diagnosis" shows.

For a UI change, work in this order: build, run the app, take a screenshot, read the screenshot, then give feedback.

## Screenshots

The `screenshot.ps1` script in the project root captures the window of the running app and saves it to `screenshot.png`.

```powershell
# Capture the MutinyIRC window (the app must be running)
pwsh -File screenshot.ps1
```

The script finds the window by process name, not by window title. Several unrelated windows carry "MutinyIRC" in their
title, including Rider and any terminal showing the repo path, so a title search picks the wrong one. It captures with
`PrintWindow`, which reads the window's own buffer: an occluded or background window still captures correctly, the
window keeps focus, and a maximized window stays maximized.

On success the script prints the process, PID, window title, and pixel size it captured. Check that line. If it names
anything other than `MutinyIRC.UI`, the image is not your app.

Then read `screenshot.png` with the Read tool. To target a different app, pass `-ProcessName`. To pick between several
running processes of the same name, pass `-WindowTitle`:

```powershell
pwsh -File screenshot.ps1 -ProcessName "SomeApp" -WindowTitle "part of the title"
```

`-WindowTitle` matches only each process's main window. It cannot select a dialog, such as Settings, inside a running
app. To capture a dialog, use the FlaUI MCP server below.

## FlaUI MCP server

The `flaui` MCP server drives the app through UI Automation. Its `windows_snapshot` gives you the accessibility tree
with element refs, so you can check a control's name and state instead of reading them off a bitmap. Prefer it when you
need to verify behavior.

### Driving the app

- **Connection state** is a text element in the tree: "Connecting…", "Connected", "Connected · TLS",
  "Reconnecting · retry 1", "Disconnected" (`ServerStrings.resx`). Read it with `windows_get_text`.
- **To send a line**, use `windows_fill` on the input box, then `windows_click` on the Send button. On the input box,
  `windows_type` fails with "Unexpected HRESULT ... COM component" and `windows_click` throws
  `NoClickablePointException`.
- **Sidebar entries** are Avalonia `Grid`s. `windows_click` on one reports "Invoked Avalonia.Controls.Grid" and
  switches the panel. The refs change after each switch, so take a fresh snapshot.

### Captures

For plain captures, prefer `screenshot.ps1`. `windows_screenshot` with `background: true` does render an occluded
Avalonia window correctly, but it reports success in two cases where it captured nothing useful:

- **Refs are per-session and reorder.** `windows_list_windows` hands out `w1`, `w2`, and so on in roughly z-order, and
  the numbering changes between sessions and whenever a window's state changes. The same MutinyIRC window has come back
  as `w3`, `w2`, and `w10` across consecutive runs. Never reuse a ref you saw earlier. Call `windows_list_windows` and
  pick the entry whose process name is `MutinyIRC.UI`, because Rider and any terminal showing the repo path also carry
  "MutinyIRC" in their titles.
- **A minimized window captures as a sliver.** You get a title-bar strip of a few hundred bytes, and the response still
  says `Screenshot saved`. Restore the window first. `screenshot.ps1` handles this with an `IsIconic` check and
  `SW_RESTORE`, and leaves every other window state alone so a maximized window stays maximized.

Neither case raises an error, and there is no equivalent of the script's flat-colour warning, so a failed capture looks
like a valid PNG. Check the image size and content before you trust it.

## Live server

### The user's settings

The app reads `servers.json` (saved networks) and `settings.json` from `MutinyIRC\` under
`Environment.SpecialFolder.Personal` (`IrcSettingsManager.cs`, `AppSettings.cs`). That folder can be redirected away
from `%USERPROFILE%\Documents`, so look it up:

```powershell
Join-Path ([Environment]::GetFolderPath('Personal')) 'MutinyIRC'
```

Read `servers.json` to learn which network auto-connects and which channels it auto-joins. That is the live server the
app dials on start.

### Observer client

`tools/irc-observer.ps1` joins a channel as a second client and logs every line the server sends, so you see what other
clients see. Start it in the background before the app joins, and keep the log in the scratchpad:

```powershell
pwsh -File tools/irc-observer.ps1 -Channel '#MutinyIRC' -Minutes 5 -LogPath <scratchpad>/observer.log
```

Parameters: `-Server` (default `irc.libera.chat`), `-Port` (default 6667, plain text only), `-Channel`, `-Minutes`,
`-LogPath`. Read the log as UTF-8, or non-ASCII text shows as mojibake:

```powershell
Get-Content <scratchpad>/observer.log -Encoding utf8 | Select-String 'PRIVMSG|QUIT'
```

The log records each client's public IP in its hostmask. Keep observer logs in the scratchpad, and copy no lines from
them into docs or commits.

### Checks

- **Non-ASCII round trip:** send `café naïve 日本語 ✓` to the channel and read the observer log for it. `/msg <own nick>`
  with the same text shows the echo in the app.
- **Reconnect and rejoin:** `/raw QUIT :msg` makes the server drop the connection. The status goes to
  "Reconnecting · retry 1", then back to "Connected", and the app rejoins its channels.
- **User teardown:** `/quit <msg>`. Libera shows `Quit: Client Quit` instead of the custom text for a new connection,
  so a missing custom message on Libera is not a bug by itself.

### TLS

The port decides TLS (`IrcPorts.IsTlsPort`): 6665 and 6667 are plain text, and every other port is TLS. `/server`
with no port dials 6697. So you can test both modes from the input box, with no change to `servers.json`:

- `/server irc.libera.chat` dials TLS on 6697. The status reads "Connected · TLS".
- `/server irc.libera.chat 6667` dials plain text. The status reads "Connected".

`/server` replaces the network's entry points for this session only. On the next start, the app dials what
`servers.json` holds.

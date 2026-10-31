# TLS toggle or port inference: prior art in IRC clients

Question: do IRC clients have an explicit TLS control, or do they infer TLS from the port?

## Answer

Every client checked turns TLS on explicitly. None infers it from the port number alone. Many use
TLS to choose the *default* port, which is the reverse of the MutinyIRC Add box today (there, the
port decides TLS through `IrcPorts.IsTlsPort`).

| Client | How TLS is turned on | Port and TLS |
|---|---|---|
| irssi | `-tls` / `-notls` on `/connect` and `/server add` | No documented inference from 6697. |
| WeeChat 4.0+ | `tls` server option, on by default since 4.0 (RFC 7194); `-notls` turns it off | Defaults to 6697 with TLS; `irc.libera.chat/7000` keeps TLS on; 6667 also needs `-notls`. |
| HexChat | Per-network "Use SSL for all the servers on this network", or `+` before the port (`host/+6697`) | 6697 is only the documented default SSL port; a bare 6697 is not said to turn SSL on. |
| mIRC | `/server -e`, or `+` before the port (`host:+6697`); `*6667` means STARTTLS | TLS is explicit, in the port syntax. |
| ZNC | `+` before the port in the server line (`host +6697`) | From setup output and guides, not official docs. |
| Halloy | `use_tls` (default `true`) | The default port follows `use_tls`: 6697 on, 6667 off. |
| The Lounge | `tls` checkbox in the network form, separate from `rejectUnauthorized` | 6697/6667 is only a convention in the docs. |
| Polari | "Use secure connection" checkbox (added 2016) | The default port follows the saved flag; toggling does not rewrite the port. |
| Quassel | "Use SSL" checkbox; you type 6697 yourself | Not confirmed. |

## A cautionary case: Smuxi

The handler for Smuxi's encryption checkbox reset the port to 6697. It also ran when the config
loaded, so a saved custom TLS port was overwritten. The fix applied the saved port after the TLS
state was read.

## Implications

- An independent TLS control is the norm, and TLS on a non-standard port is a real case (WeeChat's
  own example uses 7000).
- The common shape: TLS decides the default port, and an explicit port overrides it. A TLS toggle
  must never silently rewrite a port that the user typed.
- Text-input clients (mIRC, HexChat, ZNC) carry TLS in the host:port syntax with a `+` prefix. That
  is a way for the Add box and `/server` to express TLS on any port without a checkbox.

## Sources

- [irssi help: connect](https://irssi.org/documentation/help/connect/)
- [WeeChat spec 2023-004: TLS options and connections](https://specs.weechat.org/specs/2023-004-tls-options-and-connections.html)
- [WeeChat issue #1903](https://github.com/weechat/weechat/issues/1903)
- [HexChat settings docs](https://hexchat.readthedocs.io/en/latest/settings.html)
- [HexChat guide (LibrePlanet)](https://libreplanet.org/wiki/LibrePlanetItalia/miniguida-freenode.en)
- [Using SSL with mIRC](https://mirc.co.uk/ssl.html)
- [AfterNET SSL help](https://afternet.org/help/connecting/ssl)
- [ZNC on Debian (Akamai guide)](https://www.akamai.com/cloud/guides/install-znc-debian/)
- [Rizon wiki: SSL](https://wiki.rizon.net/index.php?title=SSL)
- [Halloy server config](https://halloy.chat/configuration/servers.html)
- [The Lounge configuration](https://thelounge.chat/docs/configuration)
- [The Lounge NetworkForm.vue](https://www.github.com/thelounge/thelounge/blob/f7926267d97cb8757aa191a23192e06d1a6a2276/client/components/NetworkForm.vue)
- [Polari TLS commit](https://lists.gnome.org/archives/commits-list/2016-February/msg06009.html)
- [FreedomBox Quassel manual](https://wiki.debian.org/FreedomBox/Manual/Quassel)
- [Smuxi issue 549](https://smuxi.im/issues/show/549)

# MutinyIRC

Mutiny is a modern, cross-platform IRC client made with heavy IRC users in mind. Formerly known as OrtzIRC, it runs on
.NET 8 with an Avalonia frontend.

Commands are loaded as plugins from a `plugins/` folder, so you can drop in your own without touching the core.

## Commands

Type these in any input box, prefixed with `/`. Arguments in `<angle brackets>` are required, `[square brackets]` are
optional. Most commands work from a channel, server, or private-message window and route to that window's server.

| Command | Syntax | What it does |
| --- | --- | --- |
| `/join` | `/join <#channel> [key]` | Join a channel, optionally with a key. |
| `/part` | `/part [#channel] [message]` | Leave a channel. Bare `/part` leaves the current one with a random part message. |
| `/say` | `/say <message>` | Send a message to the current channel or PM (the same as just typing). |
| `/me` | `/me <action>` | Send a CTCP ACTION ("emote") to the channel or PM. |
| `/msg` | `/msg <nick> <message>` | Send a private message, opening a PM tab. Messages to services echo in the server window. |
| `/query` | `/query <nick>` | Open a PM tab for a nick without sending anything. |
| `/notice` | `/notice <target> <message>` | Send a NOTICE to a nick or channel. No PM tab — it's echoed in the server window. |
| `/topic` | `/topic [new topic]` | Show the channel topic, or set it when you supply text. |
| `/kick` | `/kick <nick> [reason]` | Kick a user from the current channel. |
| `/mode` | `/mode [target] <modes...>` | View or change channel/user modes. In a channel the target is implicit (`/mode +o nick`); bare `/mode` shows the current modes. |
| `/nick` | `/nick <newnick>` | Change your nick. |
| `/whois` | `/whois <nick>` | Look up information about a user. |
| `/away` | `/away [message]` | Mark yourself away with a message; bare `/away` clears it. |
| `/server` | `/server [-n] <host> [port]` | Connect to a server (port defaults to 6667). `-n` opens it in a new window instead of reconnecting in place. |
| `/raw` | `/raw <command>` | Send a line straight to the server, unparsed (e.g. `/raw WHOIS someone`). |

## FlamingIRC

FlamingIRC is a .NET IRC framework. It is forked and heavily modified
from [Thresher](https://sourceforge.net/projects/thresher/) by Aaron Hunter.

Random quit/part messages
-------------------------

When you quit a server or part a channel from the UI without specifying a
message, Mutiny picks a random one from a pool stored at:

    <Documents>\MutinyIRC\random-messages.json

(`<Documents>` is `Environment.SpecialFolder.Personal` -- e.g.
`C:\Users\<you>\Documents` on Windows.)

The file is a JSON object keyed by message type ("quit", "part"), each
mapping to a list of strings:

    {
      "quit": ["Leaving", "Rage quit"],
      "part": ["See ya", "brb"]
    }

If the file is missing or a pool is empty, Mutiny falls back to a built-in
default ("MutinyIRC" for quit, "Goodbye!" for part). The file is loaded at
startup and saved on shutdown.

Contributors
------------

- Brian Ortiz (ortzinator)
- Max Schmeling (schmeling88)
- gparent

Also a special thanks to Adam Caudill for a lot of valuable advice and guidance provided in the early days of this
project.

License
-------

Mutiny is licensed under the MIT License (see `LICENSE`).

Note: The FlamingIRC subproject (the `FlamingIRC/` directory) is licensed
separately under the GNU General Public License v2.0 or later. See
`FlamingIRC/LICENSE` for its full terms.
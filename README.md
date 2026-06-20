# MutinyIRC

Mutiny is a modern, cross-platform IRC client made with heavy IRC users in mind. Formerly known as OrtzIRC, it runs on
.NET 8 with an Avalonia frontend.

Commands are loaded as plugins from a `plugins/` folder, so you can drop in your own without touching the core.

## Commands

Type these in any input box, prefixed with `/`. Arguments in `<angle brackets>` are required, `[square brackets]` are
optional. Most commands work from a channel, server, or private-message window and route to that window's server.

| Command | Syntax | What it does |
| --- | --- | --- |
| `/away` | `/away [message]` | Mark yourself away with a message; bare `/away` clears it. |
| `/ban` | `/ban [#channel] [-k] [-r] <nick\|mask> [reason]` | Ban a user or mask. Give a bare nick and it's resolved to `*!*@host` from the user list (falling back to `nick!*@*`); anything with `!`, `@`, or `*` is used as a literal mask. `-k` also kicks them; `-r` removes the ban instead of setting it. |
| `/invite` | `/invite <nick> [#channel]` | Invite a user to a channel. Bare `/invite <nick>` from a channel invites them to it. |
| `/join` | `/join <#channel> [key]` | Join a channel, optionally with a key. |
| `/kick` | `/kick <nick> [reason]` | Kick a user from the current channel. |
| `/me` | `/me <action>` | Send a CTCP ACTION ("emote") to the channel or PM. |
| `/mode` | `/mode [target] <modes...>` | View or change channel/user modes. In a channel the target is implicit (`/mode +o nick`); bare `/mode` shows the current modes. |
| `/msg` | `/msg <nick> <message>` | Send a private message, opening a PM tab. Messages to services echo in the server window. |
| `/nick` | `/nick <newnick>` | Change your nick. |
| `/notice` | `/notice <target> <message>` | Send a NOTICE to a nick or channel. No PM tab — it's echoed in the server window. |
| `/part` | `/part [#channel] [message]` | Leave a channel. Bare `/part` leaves the current one with a random part message. |
| `/query` | `/query <nick> [message]` | Open a PM tab for a nick, optionally sending an initial message. |
| `/quit` | `/quit [message]` | Disconnect from the current server. Bare `/quit` uses a random quit message. |
| `/raw` | `/raw <command>` | Send a line straight to the server, unparsed (e.g. `/raw WHOIS someone`). |
| `/say` | `/say <message>` | Send a message to the current channel or PM (the same as just typing). |
| `/server` | `/server [-n] <host> [port]` | Connect to a server (port defaults to 6667). `-n` opens it in a new window instead of reconnecting in place. |
| `/topic` | `/topic [new topic]` | Show the channel topic, or set it when you supply text. |
| `/whois` | `/whois <nick>` | Look up information about a user. |

## Writing your own commands

A command is a class marked `[Plugin]` with one or more `Execute` methods. The first parameter is always the *context*
the command was run from, and the rest are the arguments the user typed. The three contexts — `Channel`, `Server`, and
`PrivateMessageSession` — all derive from `MessageContext`, and every one of them exposes an `OwningServer`, so most
commands never need to know which window they were called from:

```csharp
[Plugin]
public class Nick : ICommand
{
    // Works from any window — /nick just needs the connection.
    public void Execute(MessageContext context, string nick)
        => context.OwningServer.ChangeNick(nick);
}
```

Type the context as `MessageContext` for the common case where a command behaves the same everywhere. Type it as a
concrete context when a command only makes sense in one kind of window: declaring `Execute(Channel, …)` and nothing
else means the command simply won't match from a server or PM window — no extra guard code required.

When you supply several overloads, the dispatcher picks one in two steps:

1. **More arguments win.** `/query nick hello` prefers `Execute(_, string, string)` over `Execute(_, string)`.
2. **At equal argument counts, the more-specific context wins.** An `Execute(Channel, …)` is tried before an
   `Execute(MessageContext, …)` of the same shape.

That second rule is the **general default + specific override** pattern: offer a broad `MessageContext` overload for the
usual behavior, then add a same-shaped overload for one window to specialize it. The specific one wins where it
applies, and everything else falls through to the default:

```csharp
[Plugin("Hello")]
public class Hello : ICommand
{
    // Default: greet from a server or PM window.
    public CommandResultInfo Execute(MessageContext context, string name)
        => CommandResultInfo.Success($"Hello, {name}!");

    // Override: in a channel, welcome them to it by name.
    public CommandResultInfo Execute(Channel channel, string name)
        => CommandResultInfo.Success($"Hello {name}, welcome to {channel.Name}!");
}
```

You're free to use the same trick across different argument counts — `/invite` pairs an `Execute(Channel, string)` (so
`/invite nick` from a channel invites them to *that* channel) with a general `Execute(MessageContext, string,
ChannelInfo)` for the explicit `/invite nick #channel` form.

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
# MutinyIRC

Mutiny is a modern, cross-platform IRC client made with heavy IRC users in mind. Formerly known as OrtzIRC, it runs on
.NET 8 with an Avalonia frontend.

Commands are loaded as plugins from a `plugins/` folder, so you can drop in your own without touching the core.

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

License
-------

Mutiny is licensed under the MIT License (see `LICENSE`).

Note: The FlamingIRC subproject (the `FlamingIRC/` directory) is licensed
separately under the GNU General Public License v2.0 or later. See
`FlamingIRC/LICENSE` for its full terms.
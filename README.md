Mutiny is a modern IRC client made with heavy IRC users in mind.

Formerly known as OrtzIRC.

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
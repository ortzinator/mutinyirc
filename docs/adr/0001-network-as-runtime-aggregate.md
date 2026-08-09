---
status: proposed
---

# Network is a runtime aggregate, identified by its entry points

MutinyIRC treated a network as saved configuration only: `NetworkSettings` grouped hosts and autojoin
channels on disk, while at runtime a flat list of `MutinyIRC.Common.Server` connections owned everything,
so a dropped socket took its channels and their panels with it. We decided a network is a live domain
object that owns its channels and holds at most one connection at a time, so that reconnecting through a
different host is an internal detail of the network rather than the end of the user's session. Because a
network's name is whatever the server reports in the `NETWORK` token of its `005` reply, and can therefore
change on any connect, a network is identified by its set of entry points instead.

See `GLOSSARY.md` for the definitions of network, connection, and entry point.

## Considered Options

**Keep networks as configuration only.** Cheapest, and it matches the code as written. Rejected because
channels are already saved against the network rather than the host, so the config and the runtime graph
contradicted each other, and nothing could survive a reconnect.

**Identify a network by its name.** The obvious reading, and what `NetworkSettings.Equals` does today.
Rejected because the server owns the name: connect to a network you saved as "Libera Chat", let the server
report `Libera`, and it becomes indistinguishable from a network you already had. Identity that a remote
party can change is not identity.

**Let a network hold several connections at once.** Today's accidental behaviour — two hosts of one network
give two connections that autojoin the same channel list. Rejected because it makes "the channels of a
network" ambiguous and duplicates every panel.

## Consequences

Every connection belongs to exactly one network, so dialing a host that no saved network lists mints one.
Lookup is by entry point and merging is by reported name: an ad-hoc network whose server reports a name
matching an existing network donates its entry point to that network rather than standing alone.

A hostname belongs to at most one network. This is now definitional, which is what lets
`IrcSettingsManager.GetNetwork(Server)` scan every network's hosts and stop at the first URL match.

`NetworkSettings.Equals` compares names and contradicts this decision. `MutinyIRC.Common.Server` models a
connection, not an entry point or a network, and its name is the source of the confusion this decision
resolves.
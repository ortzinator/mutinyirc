---
status: accepted
---

# Pinning a host that no saved network lists mints its network

A [pinned certificate](../../GLOSSARY.md#pinned-certificate) belongs to an [entry point](../../GLOSSARY.md#entry-point),
and an entry point is saved as part of a [network](../../GLOSSARY.md#network). When you dial a host that no saved
network lists (`/server` or the Add Server box), MutinyIRC mints its network at registration, in
`ServerViewModel.ResolveNetwork`. A host whose certificate is refused never registers, so when the refusal offers to
pin the certificate, there is no entry point to hold the pin. We decided that the pin action mints and saves the
network at that time: a network named after the host, with one entry point (the host, port, and TLS flag that were
dialed) that holds the pin. ADR 0001 already says that dialing a host that no saved network lists mints a network.
This decision moves that step earlier for a refused host.

## Considered Options

**Keep the pin in memory until registration.** The connection holds the pin, and `ResolveNetwork` writes it to the
entry point that it mints or merges. ADR 0001 stays as it is, but the certificate callback must look in two places
for a pin, and a host that never registers loses its pin when you exit.

**A per-command override** such as `/server -insecure`. It does not need a saved entry point, but it is a second,
temporary trust path that nothing saves and that the refusal cannot explain. Rejected in the same decision.

## Consequences

`ResolveNetwork` finds a saved host by its entry point first, so a network minted at pin time would never merge by
its reported name. A self-signed host that reports `Libera` would stay alone and not join your saved Libera network.
Thus the merge by reported name also applies to a network that was minted at pin time and still has only one
entry point: if the server reports the name of a different saved network, the entry point moves to that network and
keeps its pin. To find such a network at registration, MutinyIRC must know that the pin action minted it.

A refused host that you pin stays in your saved networks, even if it never registers. You remove it the same way
as any other network.

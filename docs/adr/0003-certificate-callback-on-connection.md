---
status: accepted
---

# The certificate decision enters FlamingIRC through a public callback on Connection

MutinyIRC needs to accept a certificate that fails the normal check when the per-network "Verify TLS
certificates" switch is off or when the certificate matches a [pinned certificate](../../GLOSSARY.md#pinned-certificate).
We decided that FlamingIRC exposes this as a public delegate property,
`Connection.CertificateValidationFailedCallback`, and that FlamingIRC itself reports the rejected certificate
(DER bytes, `SslPolicyErrors`, and a fingerprint helper) on `ConnectFailedEventArgs`. FlamingIRC is a framework
with other callers, and every client that talks TLS to self-signed IRC servers needs the same two things: a
way to say yes, and a way to show the user what it said no to.

## Considered Options

**A `Connection` subclass in MutinyIRC.Common** that overrides the protected `OnCertificateValidationFailed`.
It adds no public API, but every other caller of the framework must subclass to get the same behavior, and the
rejected certificate is still lost unless each subclass captures it.

**Declarative trust settings on `ConnectionArgs`** (a verify flag and a list of pins), applied by FlamingIRC.
It is the least work for a caller, but it puts MutinyIRC's trust policy (whole-certificate SHA-256, pins as a
fallback, see ADR 0002) into a public framework that other clients may want to use differently.

## Consequences

The callback runs only when the normal check fails. A caller cannot use it to reject a certificate that passes
the normal check, so strict pinning is not possible through it. A separate member can add that later without a
break.

FlamingIRC knows nothing about pins. MutinyIRC.Common finds a changed pin from the facts it has: the certificate
was refused and the host already has a pin.

The callback runs on a thread-pool thread during the TLS handshake. MutinyIRC's callback reads the network's
switch and pins at that time, so the store that holds them must be safe to read from that thread.

The callback is on `Connection` only, not on `IConnection`, because a new interface member breaks external
implementers.

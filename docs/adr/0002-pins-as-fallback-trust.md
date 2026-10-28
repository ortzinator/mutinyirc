---
status: accepted
---

# A pinned certificate is a fallback, not a stricter check

MutinyIRC refuses a TLS server whose certificate fails the normal chain and hostname check, which makes
self-signed servers such as a home bouncer unusable without turning verification off. We decided that a
[pinned certificate](../../GLOSSARY.md#pinned-certificate) is a fallback: a certificate is accepted when it
passes the normal check *or* when its SHA-256 fingerprint matches the pin. Pinning exists to make
self-signed servers usable without giving up all protection, and a fallback does that without ever
refusing a certificate that passes the normal check. The per-network "Verify TLS certificates" switch
stays as the last resort for servers that a pin cannot serve.

## Considered Options

**Replace (WeeChat, soju).** Accept only the pinned certificate and ignore the normal check. Stronger for a
pinned host, but a pinned server that moves to a CA-issued certificate is refused until you pin again.

**Add (irssi).** Require the normal check *and* the pin. Defends against a mis-issued CA certificate, but a
Let's Encrypt certificate is renewed every 45 to 90 days, and each renewal breaks the pin.

## Consequences

An attacker who gets a certificate that passes the normal check for a pinned host is accepted. We accept
this risk: the same attacker is accepted for every host that has no pin.

A certificate that fails the normal check and does not match an existing pin is the one case where a pin
can catch an attacker, so MutinyIRC reports it apart from an unknown certificate: it shows the old and new
fingerprints, and pinning the new certificate replaces the old pin.

Changing what a pin means later changes what every saved pin accepts.

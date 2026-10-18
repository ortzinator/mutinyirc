# Prior art: TLS certificate trust in IRC clients and bouncers

Research for issue #6. It feeds the decision "Choose the certificate trust model". This file records facts only. It
does not make the decision.

Sources are official docs or source code. Source links are pinned to the commit that was read (October 2026), so line
numbers stay valid.

## Summary table

| Client | Off switch | Pinning | TOFU prompt | Granularity | What is pinned, hash | Pin vs. CA check |
| --- | --- | --- | --- | --- | --- | --- |
| HexChat | Yes ("Accept invalid SSL certificates") | No | No | Per network | n/a | n/a |
| WeeChat | Yes (`tls_verify`, default on) | Yes (`tls_fingerprint`) | No | Per server section (one network) | Whole leaf cert; SHA-512, SHA-256 or SHA-1; list allowed | Pin **replaces** CA check |
| irssi | Yes (`-tls_verify`, default on since 1.4.1) | Yes (`-tls_pinned_cert`, `-tls_pinned_pubkey`) | No | Per server entry | Whole leaf cert or SPKI; SHA-256 only | Pin is **added** to CA check |
| The Lounge | Yes (`rejectUnauthorized`, "Only allow trusted certificates") | No | No | Per network | n/a | n/a |
| soju | No (only plain-text `irc+insecure://`) | Yes (`-certfp`) | No (error shows the fingerprint) | Per network | Whole cert, any cert in the chain; SHA-512 or SHA-256 | Pin **replaces** CA check |
| ZNC | Yes (`TrustAllCerts`) | Yes (`AddTrustedServerFingerprint`, a set) | Semi: error prints the exact command to run | Per network | Whole leaf cert; SHA-256 | Pin is an **alternative** that passes when the CA check fails |
| Quassel | Yes (`sslVerify` per server, default on) | No | No | Per server | n/a | n/a |
| Halloy | Yes (`dangerously_accept_invalid_certs`, default off) | No (custom root cert instead) | No | Per server | n/a | n/a |

No client checked here does interactive trust on first use (a dialog that asks "trust this certificate?" and remembers
the answer). ZNC comes closest: it refuses, prints the SHA-256 fingerprint and the exact command to trust it, and the
user runs that command. soju prints the SHA-512 fingerprint in its error for the same reason.

## Per-client findings

### HexChat

- The project is archived on GitHub (last push 2024-08-19), so this behavior is final.
  ([repo](https://github.com/hexchat/hexchat))
- One switch per network: the Network List edit dialog has a check box "Accept invalid SSL certificates" that maps to
  `FLAG_ALLOW_INVALID`.
  ([servlistgui.c#L1886](https://github.com/hexchat/hexchat/blob/b544ac3350e85d4cc41fe3414cbdb82d75ce5d7a/src/fe-gtk/servlistgui.c#L1886))
  On connect, the flag sets `serv->accept_invalid_cert`.
  ([server.c#L419-L420](https://github.com/hexchat/hexchat/blob/b544ac3350e85d4cc41fe3414cbdb82d75ce5d7a/src/common/server.c#L419-L420))
- With the flag on, HexChat ignores only a fixed list of OpenSSL errors: hostname mismatch, unknown issuer, unable to
  verify leaf, self-signed leaf, self-signed in chain, and expired. It prints the error with " -- Ignored". Any other
  verify error still fails the connection. With the flag off, any error fails the connection with a "connection
  failed" event that shows the OpenSSL error string and code.
  ([server.c#L616-L660](https://github.com/hexchat/hexchat/blob/b544ac3350e85d4cc41fe3414cbdb82d75ce5d7a/src/common/server.c#L616-L660))
- No pinning and no fingerprint storage.

### WeeChat

- Server option `tls_verify`, default `on`, described as "check that the TLS connection is fully trusted". Server
  option `tls_fingerprint`, default empty.
  ([irc-server.c#L81-L82](https://github.com/weechat/weechat/blob/95afbb1784db26935953e1c34169efd44c13dfa9/src/plugins/irc/irc-server.c#L81-L82),
  [irc-config.c#L2031-L2061](https://github.com/weechat/weechat/blob/95afbb1784db26935953e1c34169efd44c13dfa9/src/plugins/irc/irc-config.c#L2031-L2061))
- `tls_fingerprint` text: "fingerprint of certificate which is trusted and accepted for the server; only hexadecimal
  digits are allowed (0-9, a-f): 128 chars for SHA-512, 64 chars for SHA-256, 40 chars for SHA-1 (insecure, not
  recommended); many fingerprints can be separated by commas; if this option is set, the other checks on certificates
  are NOT performed (option "tls_verify")".
  ([irc-config.c#L2031-L2045](https://github.com/weechat/weechat/blob/95afbb1784db26935953e1c34169efd44c13dfa9/src/plugins/irc/irc-config.c#L2031-L2045))
- The hash is chosen from the length of each hex string. It is a hash of the whole certificate
  (`gnutls_x509_crt_get_fingerprint`), checked against the first certificate the server sends (the leaf) only.
  ([irc-server.c#L136-L143](https://github.com/weechat/weechat/blob/95afbb1784db26935953e1c34169efd44c13dfa9/src/plugins/irc/irc-server.c#L136-L143),
  [irc-server.c#L4917-L4995](https://github.com/weechat/weechat/blob/95afbb1784db26935953e1c34169efd44c13dfa9/src/plugins/irc/irc-server.c#L4917-L4995),
  [irc-server.c#L5112-L5120](https://github.com/weechat/weechat/blob/95afbb1784db26935953e1c34169efd44c13dfa9/src/plugins/irc/irc-server.c#L5112-L5120))
- With a fingerprint set, WeeChat skips the expiry, activation-date, hostname, and CA checks. On a match it prints
  "certificate fingerprint matches". On a mismatch it prints "certificate fingerprint does NOT match (check value of
  option irc.server.NAME.tls_fingerprint)" and fails.
  ([irc-server.c#L5143-L5193](https://github.com/weechat/weechat/blob/95afbb1784db26935953e1c34169efd44c13dfa9/src/plugins/irc/irc-server.c#L5143-L5193))
- On every connect WeeChat prints one line of info per received certificate, and messages such as "peer's certificate
  is NOT trusted", "issuer is unknown", "has been revoked", "hostname ... does NOT match".
  ([irc-server.c#L5127-L5250](https://github.com/weechat/weechat/blob/95afbb1784db26935953e1c34169efd44c13dfa9/src/plugins/irc/irc-server.c#L5127-L5250))
- `tls_verify = off` turns every failure into a pass, the fingerprint mismatch too. The code comment reads "An error
  should stop the handshake unless the user doesn't care."
  ([irc-server.c#L5363-L5368](https://github.com/weechat/weechat/blob/95afbb1784db26935953e1c34169efd44c13dfa9/src/plugins/irc/irc-server.c#L5363-L5368))
- Global trust roots: `weechat.network.gnutls_ca_system` (default on, loads the system CAs) and
  `weechat.network.gnutls_ca_user` (extra CA files).
  ([core-config.c#L5377-L5395](https://github.com/weechat/weechat/blob/95afbb1784db26935953e1c34169efd44c13dfa9/src/core/core-config.c#L5377-L5395))
- Renewal: a whole-cert pin breaks on every renewal. The comma list lets the user add the new fingerprint before the
  old one is removed, but WeeChat does nothing to help with this.

### irssi

- `/CONNECT` and `/SERVER ADD` options: `-tls_verify` "Verifies the TLS certificate of the server",
  `-tls_pinned_cert` "Pinned x509 certificate fingerprint", `-tls_pinned_pubkey` "Pinned public key fingerprint",
  plus `-tls_cafile` and `-tls_capath`. ([help connect](https://irssi.org/documentation/help/connect/))
- `-tls_verify` is on by default since 1.4.1 (2022-06-12). Old configs that saved `tls_verify = "no"` keep it off.
  ([NEWS#L136-L142](https://github.com/irssi/irssi/blob/43f1727ef9fe1cb51d5031ee2dd0e1965217e4cb/NEWS#L136-L142),
  [servers-setup.c#L519-L520](https://github.com/irssi/irssi/blob/43f1727ef9fe1cb51d5031ee2dd0e1965217e4cb/src/core/servers-setup.c#L519-L520))
- Pinning came in 1.1.0 (2018-01-15). Both pins are SHA-256: the cert pin is `X509_digest(cert, EVP_sha256())`, the
  public-key pin is SHA-256 over the DER `SubjectPublicKeyInfo` (`i2d_X509_PUBKEY`). That is the same SPKI hash HPKP
  used. Irssi prints both values when it connects to a TLS server. The NEWS entry gives the `openssl` commands to
  compute them by hand.
  ([NEWS#L775-L794](https://github.com/irssi/irssi/blob/43f1727ef9fe1cb51d5031ee2dd0e1965217e4cb/NEWS#L775-L794),
  [network-openssl.c#L883-L900](https://github.com/irssi/irssi/blob/43f1727ef9fe1cb51d5031ee2dd0e1965217e4cb/src/core/network-openssl.c#L883-L900))
- Pins are checked first, then the normal CA and hostname check if `tls_verify` is on. A pin does not replace the CA
  check; it is an extra condition. A mismatch logs "Pinned certificate mismatch" or "Pinned public key mismatch" and
  fails the connection.
  ([network-openssl.c#L917-L941](https://github.com/irssi/irssi/blob/43f1727ef9fe1cb51d5031ee2dd0e1965217e4cb/src/core/network-openssl.c#L917-L941))
- One value per pin (string compare, no list).
- Renewal: the SPKI pin survives a renewal that keeps the key. The cert pin breaks on every renewal.

### The Lounge

- One boolean per network, `rejectUnauthorized`: "Whether the server certificate should be verified against the list
  of supplied Certificate Authorities (CAs) by your Node.js installation". The shipped config sets it to `true`.
  ([configuration docs](https://thelounge.chat/docs/configuration),
  [defaults/config.js#L238-L271](https://github.com/thelounge/thelounge/blob/70db8bcb6faf98e9c2a09018e26c28ae3eaa0b09/defaults/config.js#L238-L271))
- The network form shows it as a check box "Only allow trusted certificates".
  ([NetworkForm.vue#L89-L96](https://github.com/thelounge/thelounge/blob/70db8bcb6faf98e9c2a09018e26c28ae3eaa0b09/client/components/NetworkForm.vue#L89-L96))
- The model default is `false` when the attribute is missing.
  ([network.ts#L149](https://github.com/thelounge/thelounge/blob/70db8bcb6faf98e9c2a09018e26c28ae3eaa0b09/server/models/network.ts#L149))
- If the host has an active IRCv3 STS policy, The Lounge forces TLS and sets `rejectUnauthorized = true`.
  ([network.ts#L285-L298](https://github.com/thelounge/thelounge/blob/70db8bcb6faf98e9c2a09018e26c28ae3eaa0b09/server/models/network.ts#L285-L298))
- Error UX: the Node TLS error goes to the network lobby as "Socket error: ...".
  ([connection.ts#L219-L227](https://github.com/thelounge/thelounge/blob/70db8bcb6faf98e9c2a09018e26c28ae3eaa0b09/server/plugins/irc-events/connection.ts#L219-L227))
- No pinning.

### soju (bouncer)

- `network create/update -certfp <fingerprint>`: "Instead of using certificate authorities to check the server's TLS
  certificate, check whether the server certificate matches the provided fingerprint. This can be used to connect to
  servers using self-signed certificates." The man page says the format is SHA-512 and an empty string removes the
  pin. ([soju(1)](https://soju.im/doc/soju.1.html))
- The code accepts `sha-512:` or `sha-256:` prefixed values. It sets `InsecureSkipVerify` and checks the hash of
  **each** raw certificate in the presented chain, so the user can pin the leaf or an issuer certificate. On a
  mismatch the error is "the configured TLS certificate fingerprint doesn't match the server's - HEX", where HEX is the
  SHA-512 of the leaf, so the user can copy it.
  ([upstream.go#L290-L321](https://codeberg.org/emersion/soju/src/commit/3dec1ed4474263ed6057abb665731e61b2da1b11/upstream.go#L290-L321))
- No "verify off" switch. The only other choice is plain text (`irc+insecure://`).
  ([soju(1)](https://soju.im/doc/soju.1.html))
- Renewal: a leaf pin breaks on every renewal. Because any certificate in the chain may match, a pin on a self-run
  private CA certificate survives leaf renewals.

### ZNC (bouncer)

- Per network: `TrustAllCerts` (default false), `TrustPKI` (default true), and a set of trusted fingerprints.
  ([IRCNetwork.cpp#L140-L149](https://github.com/znc/znc/blob/881de2f4d5c87bed7d7480125da6daabd1746c2d/src/IRCNetwork.cpp#L140-L149),
  [IRCNetwork.cpp#L389-L390](https://github.com/znc/znc/blob/881de2f4d5c87bed7d7480125da6daabd1746c2d/src/IRCNetwork.cpp#L389-L390))
- Order of checks: `TrustAllCerts` passes everything. Else, if `TrustPKI` and there are no CA or hostname errors, pass.
  Else, if the SHA-256 of the leaf certificate is in the trusted set, pass. Else fail with "Invalid SSL certificate:
  <errors>". ([Socket.cpp#L113-L147](https://github.com/znc/znc/blob/881de2f4d5c87bed7d7480125da6daabd1746c2d/src/Socket.cpp#L113-L147))
- The fingerprint is `X509_digest(..., EVP_sha256())` of the whole certificate, written as hex with colons. A source
  comment notes that the socket library's own version "returns insecure SHA-1".
  ([Socket.cpp#L154-L180](https://github.com/znc/znc/blob/881de2f4d5c87bed7d7480125da6daabd1746c2d/src/Socket.cpp#L154-L180))
- Failure UX: ZNC prints the full certificate (`X509_print`), the SHA-1 and SHA-256 fingerprints, and "If you trust
  this certificate, do /znc AddTrustedServerFingerprint <sha256>".
  ([IRCSock.cpp#L1561-L1580](https://github.com/znc/znc/blob/881de2f4d5c87bed7d7480125da6daabd1746c2d/src/IRCSock.cpp#L1561-L1580))
- Renewal: with `TrustPKI` on, a CA-valid renewed certificate passes without the pin, so the pin matters only while
  the certificate is not CA-valid (self-signed). A self-signed certificate that is regenerated needs a new pin.

### Quassel

- Per server: `sslVerify`, default true ("If true, validate SSL certificates").
  ([network.h#L128-L139](https://github.com/quassel/quassel/blob/c63fc1eecc4abc130b742ffc678b22f6a852f782/src/common/network.h#L128-L139))
- On an SSL error with `sslVerify` on, the status buffer shows "Encrypted connection couldn't be verified,
  disconnecting since verification is required (Reason: ...)" and the core reconnects later. With it off, the message
  says "continuing since verification is not required" and the error is ignored.
  ([corenetwork.cpp#L1513-L1553](https://github.com/quassel/quassel/blob/c63fc1eecc4abc130b742ffc678b22f6a852f782/src/core/corenetwork.cpp#L1513-L1553))
- No pinning for IRC server connections.

### Halloy

- Per server: `dangerously_accept_invalid_certs` (default false): "When true, all certificate validations are
  skipped." Also `root_cert_path`, a PEM root certificate for that server.
  ([servers.md](https://github.com/squidowl/halloy/blob/682d734582a8b6725b8836780fb5f1469a668227/docs/configuration/servers.md))
- No pinning.

### IRCv3 STS (spec constraint)

- "Clients MUST use sufficient certificate verification to ensure the connection is secure without any errors. This
  might involve validation against a system root certificate bundle or a user-specified trust root."
- "Clients should treat a failure to establish a secure connection to an STS-enabled host as a critical error" and "the
  user should not be presented with a dialog that allows them to proceed with the connection with a click."
  ([IRCv3 STS](https://ircv3.net/specs/extensions/sts))

## Let's Encrypt renewal and pins

- Let's Encrypt certificates are valid for 90 days today. The announced schedule: the default `classic` profile drops to
  64 days on 2027-02-10 and to 45 days on 2028-02-16; the opt-in `tlsserver` profile went to 45 days on 2026-05-13.
  ([Let's Encrypt, "From 90 to 45"](https://letsencrypt.org/2025/12/02/from-90-to-45/))
- Certbot makes a new private key on each renewal by default: "Not reusing private keys is the default behavior of
  Certbot." `--reuse-key` keeps the key. ([certbot docs](https://eff-certbot.readthedocs.io/en/stable/using.html))
  Other ACME clients were not checked.
- Let's Encrypt keeps several intermediates in rotation and backup intermediates that it "may begin issuing Subscriber
  certificates from ... at any time, without warning." ([Chains of Trust](https://letsencrypt.org/certificates/))
- Consequences, from the facts above:
  - A whole-certificate pin (WeeChat, soju leaf, ZNC, irssi `tls_pinned_cert`) breaks at every renewal: every 90 days
    now, every 45 days from 2028.
  - An SPKI pin (irssi `tls_pinned_pubkey`) breaks at renewal unless the server operator reuses the key, which certbot
    does not do by default.
  - A pin on a Let's Encrypt intermediate can break without warning.
  - For a server with a CA-valid certificate, a pin adds little unless it is an extra check (irssi). The clients that
    let a pin replace the CA check (WeeChat, soju) or be a fallback (ZNC) document the pin for self-signed servers.

## .NET 8 SslStream facts

### Revocation

- `AuthenticateAsClientAsync(string targetHost)` calls the three-argument overload with
  `checkCertificateRevocation: false`.
  ([SslStream.cs#L369](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/System.Net.Security/src/System/Net/Security/SslStream.cs#L369))
  The docs agree: "The certificate revocation list is not checked during authentication."
  ([Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.net.security.sslstream.authenticateasclientasync?view=net-8.0#system-net-security-sslstream-authenticateasclientasync(system-string)))
- The `bool` overloads map `true` to `X509RevocationMode.Online` and `false` to `NoCheck`.
  ([SslStream.cs#L373-L385](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/System.Net.Security/src/System/Net/Security/SslStream.cs#L373-L385))
- `SslClientAuthenticationOptions.CertificateRevocationCheckMode` defaults to `NoCheck` and accepts `NoCheck`,
  `Offline`, or `Online`.
  ([SslClientAuthenticationOptions.cs#L14](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/System.Net.Security/src/System/Net/Security/SslClientAuthenticationOptions.cs#L14),
  [#L49-L60](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/System.Net.Security/src/System/Net/Security/SslClientAuthenticationOptions.cs#L49-L60))
- SslStream sets `chain.ChainPolicy.RevocationMode` from that option and `RevocationFlag = ExcludeRoot`. If
  `SslClientAuthenticationOptions.CertificateChainPolicy` (an `X509ChainPolicy`) is set, SslStream uses it as given and
  ignores the revocation option. That policy also allows `CustomRootTrust` with a `CustomTrustStore`.
  ([SslStream.Protocol.cs#L1041-L1071](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/System.Net.Security/src/System/Net/Security/SslStream.Protocol.cs#L1041-L1071),
  [SslClientAuthenticationOptions.cs#L98](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/System.Net.Security/src/System/Net/Security/SslClientAuthenticationOptions.cs#L98))
- On macOS, `X509RevocationMode.Offline` is treated as `Online`, and `X509ChainPolicy.UrlRetrievalTimeout` is
  ignored. ([Cross-platform cryptography, X509Chain](https://learn.microsoft.com/en-us/dotnet/standard/security/cross-platform-cryptography#x509chain))

### Validation flow and SslPolicyErrors per platform

- The flow is the same on every platform: build an `X509Chain` for the server certificate, add the server-auth EKU to
  the application policy, compute `SslPolicyErrors`, then call the user callback with (certificate, chain, errors). If
  there is no callback, success means `errors == None`.
  ([SslStream.Protocol.cs#L1010-L1100](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/System.Net.Security/src/System/Net/Security/SslStream.Protocol.cs#L1010-L1100))
- The same three flags appear on all platforms: `RemoteCertificateNotAvailable`, `RemoteCertificateNameMismatch`,
  `RemoteCertificateChainErrors`. What differs is which engine sets them:
  - **Windows**: `X509Chain.Build` over CryptoAPI. The name check is `CertVerifyCertificateChainPolicy` with
    `CERT_CHAIN_POLICY_SSL`, which ignores all policy errors except the name, so only `CERT_E_CN_NO_MATCH` sets
    `NameMismatch`.
    ([CertificateValidation.Windows.cs](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/Common/src/System/Net/Security/CertificateValidation.Windows.cs))
  - **Linux**: `X509Chain.Build` over the .NET OpenSSL chain code. The name check is OpenSSL `X509_check_host` (after
    IDN mapping) or `X509_check_ip` for an IP address. An empty host name always sets `NameMismatch`.
    ([CertificateValidation.Unix.cs](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/Common/src/System/Net/Security/CertificateValidation.Unix.cs))
  - **macOS**: `X509Chain.Build` over Apple Security. The name check is `SslCheckHostnameMatch` in the native shim.
    ([CertificateValidationPal.OSX.cs#L12-L48](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/System.Net.Security/src/System/Net/CertificateValidationPal.OSX.cs#L12-L48))
- Trust roots differ per platform:
  - Windows uses the Windows certificate store.
  - Linux uses the OpenSSL CA bundle (`SSL_CERT_FILE`, `SSL_CERT_DIR`, distro defaults, fallback `/etc/ssl/certs`).
    On Linux, `CurrentUser\Intermediate` caches intermediates that .NET downloads through AIA during chain builds.
  - macOS uses `SecTrustSettings`; certificates added to `CurrentUser\Intermediate` do not affect chain building.
  ([Cross-platform cryptography, X509Store](https://learn.microsoft.com/en-us/dotnet/standard/security/cross-platform-cryptography#x509store))
- The exact `X509ChainStatusFlags` set for the same bad certificate can differ by platform, because three different
  engines build the chain. The source above shows this. The research did not list the per-flag differences.
  Code that decides on `SslPolicyErrors` plus a fingerprint, not on specific chain status flags, does not depend on
  them.

### Lifetime of the objects the callback receives

- The callback's `certificate` is the object SslStream keeps as `RemoteCertificate`. The `chain` and every
  `chain.ChainElements[i].Certificate` are disposed in a `finally` block right after the callback returns. Copy any
  data you need from the chain (for example to show in a prompt) inside the callback.
  ([SslStream.Protocol.cs#L1082-L1131](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/System.Net.Security/src/System/Net/Security/SslStream.Protocol.cs#L1082-L1131))
- If the callback returns false, the handshake fails and the `SslStream` is unusable; the caller must close it.
  ([Microsoft Learn remarks](https://learn.microsoft.com/en-us/dotnet/api/system.net.security.sslstream.authenticateasclientasync?view=net-8.0))
  The callback runs during the handshake, so a "trust this?" prompt that waits for the user has two choices: block the
  handshake while it waits, or reject, save the certificate data, ask, and reconnect.
- For TLS 1.3 or a renegotiation with an unchanged certificate, SslStream does not call the callback again.
  ([SslStream.Protocol.cs#L1022-L1030](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/System.Net.Security/src/System/Net/Security/SslStream.Protocol.cs#L1022-L1030))

### Computing fingerprints

All APIs below exist in .NET 8.

- Whole-certificate SHA-256: `certificate.GetCertHash(HashAlgorithmName.SHA256)` (bytes) or
  `certificate.GetCertHashString(HashAlgorithmName.SHA256)` (uppercase hex, no separators). Both are on the base
  `X509Certificate`, so no cast is needed. The parameterless overloads return SHA-1.
  ([GetCertHash](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate.getcerthash?view=net-8.0),
  [GetCertHashString](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate.getcerthashstring?view=net-8.0),
  [X509Certificate.cs#L343-L370](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/System.Security.Cryptography/src/System/Security/Cryptography/X509Certificates/X509Certificate.cs#L343-L370))
  This equals `openssl x509 -fingerprint -sha256` (hash of the DER certificate).
- SPKI SHA-256: `PublicKey.ExportSubjectPublicKeyInfo()` returns the DER `SubjectPublicKeyInfo`. Hash it with
  `SHA256.HashData`. `PublicKey` is on `X509Certificate2`, so wrap the base type first:

  ```csharp
  using var cert2 = new X509Certificate2(certificate);
  byte[] spki = cert2.PublicKey.ExportSubjectPublicKeyInfo();
  byte[] spkiSha256 = SHA256.HashData(spki);
  ```

  ([ExportSubjectPublicKeyInfo](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.publickey.exportsubjectpublickeyinfo?view=net-8.0),
  [PublicKey.cs#L104-L113](https://github.com/dotnet/runtime/blob/b60797bb39c010467385f7bc4eaaa0ab53e44d81/src/libraries/System.Security.Cryptography/src/System/Security/Cryptography/X509Certificates/PublicKey.cs#L104-L113))
  This is the same input irssi hashes for `-tls_pinned_pubkey` (`i2d_X509_PUBKEY`), so the values match.

### Current FlamingIRC hook

- `TcpTextClient` calls `AuthenticateAsClientAsync(server)`, so revocation is not checked today. Its validation
  callback passes when `errors == None` and otherwise calls the overridable `OnCertificateValidationFailed`. An
  exception from the override counts as a rejection.
  ([FlamingIRC/TcpTextClient.cs](../../FlamingIRC/TcpTextClient.cs), lines 226-243 and 413-430)

## Implications for MutinyIRC

The facts the trust-model decision hangs on:

1. **No client checked has an interactive TOFU dialog.** The common pattern is a per-network or per-server verify
   switch (all eight), plus, in four of them, a stored fingerprint. ZNC and soju come closest to TOFU: they fail and
   print the fingerprint to trust.
2. **Granularity is per network or per server everywhere.** No client has only a global switch. WeeChat and Halloy
   make CA roots global or per server; the rest use the system store.
3. **Two pin semantics exist.** A pin either *replaces* the CA check (WeeChat, soju, and ZNC as a fallback) or is *added*
   to it (irssi). "Replace" serves self-signed servers. "Add" defends against a mis-issued CA certificate.
4. **Hashes:** SHA-256 is the common denominator (irssi, ZNC, soju, WeeChat). soju defaults to SHA-512. Only irssi
   offers SPKI pinning.
5. **Renewal:** whole-cert pins break on every Let's Encrypt renewal: 90 days now, 64 days from February 2027, 45 days
   from February 2028. SPKI pins break too unless the operator reuses the key, which certbot does not do by default.
   So for a CA-valid server, a pin is a maintenance cost; for a self-signed server, the pin lasts as long as the
   operator keeps the certificate or key.
6. **Pin-changed UX is "fail with a message" everywhere.** No client has a "the certificate changed, accept the new
   one?" flow.
7. **STS:** for a host with an STS policy, the spec says that a certificate failure is critical and that no one-click
   bypass should be offered. The Lounge enforces this by forcing verification on.
8. **.NET defaults:** revocation is off with the `string` overload. Turning it on is one option
   (`CertificateRevocationCheckMode`), but on macOS `Offline` acts as `Online` and the timeout is ignored.
9. **.NET mechanics:** the callback gets the same three `SslPolicyErrors` flags on Windows, Linux, and macOS, from
   different chain engines. SHA-256 cert and SPKI fingerprints are each one or two lines. The chain is disposed after
   the callback, and a false return kills the stream, so a prompt-based flow either blocks the handshake or rejects
   and reconnects.

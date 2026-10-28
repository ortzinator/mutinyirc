//
// TcpTextClient.cs
//
// Author:
//       Joshua Simmons <simmons.44@gmail.com>
//
// Copyright (c) 2009 Joshua Simmons
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlamingIRC;

/// <summary>
/// Why a connect attempt failed. Only <see cref="SocketError" /> comes with a socket error code.
/// </summary>
public enum ConnectError
{
    ConnectionRefused,

    /// <summary>
    /// The TLS handshake failed for a reason other than the server certificate, for example
    /// because the client and the server have no protocol version in common.
    /// </summary>
    AuthenticationFailed,

    /// <summary>
    /// A socket operation failed. The failure always has a socket error code.
    /// </summary>
    SocketError,

    /// <summary>
    /// The connect failed for a reason that has no socket error code and no other value here.
    /// </summary>
    Other,

    /// <summary>
    /// The server certificate did not pass validation, and
    /// <see cref="TcpTextClient.OnCertificateValidationFailed" /> did not accept it.
    /// </summary>
    CertificateRejected
}

/// <summary>
/// Why a session ended. Only <see cref="SocketError" /> comes with a socket error code.
/// </summary>
public enum DisconnectReason
{
    UserInitiated,
    PingTimeout,

    /// <summary>
    /// A read or a write failed with a socket error. When the transport ends the session for
    /// this reason, the failure always has a socket error code.
    /// </summary>
    SocketError,

    /// <summary>
    /// The server closed the connection.
    /// </summary>
    RemoteHostClosedConnection,

    /// <summary>
    /// The session ended for a reason that has no socket error code and no other value here.
    /// </summary>
    Other
}

public abstract class TcpTextClient
{
    private const int BufferLength = 512;

    /// <summary>
    /// The current session, or null when the client is idle. Only <see cref="Connect" /> sets
    /// it, and only <see cref="TryEnd" /> clears it.
    /// </summary>
    private Session _session;

    protected TcpTextClient()
    {
        TextEncoding = Encoding.UTF8;
    }

    public Encoding TextEncoding { get; protected set; }

    /// <summary>
    /// True from just before <see cref="OnConnect()" /> until the session ends.
    /// </summary>
    public bool Connected => Volatile.Read(ref _session) is { IsOpen: true };

    /// <summary>
    /// True when no session and no connect attempt is live, so <see cref="Connect" /> can
    /// start one.
    /// </summary>
    protected bool IsIdle => Volatile.Read(ref _session) == null;

    /// <summary>
    /// Start a session: connect to the server, and do the TLS handshake if
    /// <paramref name="ssl" /> is true. The result comes later, through
    /// <see cref="OnConnect()" /> or <see cref="OnConnectFailed" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">A session or a connect attempt is live.</exception>
    protected void Connect(string server, int port, bool ssl)
    {
        var session = new Session(TextEncoding);
        if (Interlocked.CompareExchange(ref _session, session, null) != null)
        {
            session.Close();
            throw new InvalidOperationException("A session or a connect attempt is live.");
        }

        Task.Run(() => RunSession(session, server, port, ssl));
    }

    /// <summary>
    /// End the current session or connect attempt.
    /// </summary>
    /// <remarks>
    /// The client need not be connected. If it is idle, this still calls
    /// <see cref="OnDisconnect" />. If a session is live, this calls <see cref="OnDisconnect" />
    /// only if no other path ended the session first.
    /// </remarks>
    public void Disconnect(DisconnectReason reason)
    {
        if (Volatile.Read(ref _session) is { } session)
        {
            Lose(session, reason);
            return;
        }

        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo,
            $"[{Thread.CurrentThread.Name}] TcpTextClient::Disconnect() idle, reason={reason}");
        OnDisconnect(reason, null);
    }

    /// <summary>
    /// Sends a string message to the server.
    /// </summary>
    /// <remarks>
    /// If the write fails, the session ends and <see cref="OnDisconnect" /> tells why. The write
    /// is synchronous, so a QUIT is in the socket before a <see cref="Disconnect" /> that
    /// follows it closes the socket.
    /// </remarks>
    /// <param name="message">A <see cref="string" /></param>
    /// <exception cref="InvalidOperationException">The client is not connected.</exception>
    public void Send(string message)
    {
        Session session = Volatile.Read(ref _session);
        if (session is not { IsOpen: true })
            throw new InvalidOperationException("The client is not connected.");

        message += "\r\n";
        try
        {
            session.Write(TextEncoding.GetBytes(message));
        }
        catch (Exception e) when (IsIoFailure(e))
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                $"[{Thread.CurrentThread.Name}] TcpTextClient::Send() exception={e.Message}");
            LoseToIoFailure(session, e);
            return;
        }

        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] TcpTextClient::Send() {message.TrimEnd()}");
    }

    /// <summary>
    /// Run one session from the connect to the end.
    /// </summary>
    /// <remarks>
    /// An I/O failure ends the session through <see cref="TryEnd" />. An exception from subclass
    /// code or an event handler is not an I/O failure, and this method does not catch it. This
    /// method is async void and runs on the thread pool, so that exception ends the process.
    /// </remarks>
    private async void RunSession(Session session, string server, int port, bool ssl)
    {
        if (await session.OpenAsync(server, port, ssl, OnCertificateValidate).ConfigureAwait(false) is { } failure)
        {
            if (TryEnd(session))
                OnConnectFailed(failure.Error, failure.SocketErrorCode, failure.RejectedCertificate);
            return;
        }

        // A Disconnect during the handshake ended this session, so do not report it as connected.
        // That Disconnect can close the session before its stream opens, so close it again here.
        if (Volatile.Read(ref _session) != session)
        {
            session.Close();
            return;
        }

        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo,
            $"[{Thread.CurrentThread.Name}] TcpTextClient::RunSession() connected to {server}, tls={ssl}");
        OnConnect();
        await ReadLines(session).ConfigureAwait(false);
    }

    /// <summary>
    /// Accept a certificate that passes the normal check. Give any other certificate to
    /// <see cref="OnCertificateValidationFailed" />.
    /// </summary>
    internal bool OnCertificateValidate(X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
    {
        if (errors == SslPolicyErrors.None)
            return true;

        try
        {
            return OnCertificateValidationFailed(certificate, chain, errors);
        }
        catch (Exception e)
        {
            // An exception from the override counts as a rejection. SslStream calls this on a
            // thread-pool thread, where an exception would end the process.
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                $"[{Thread.CurrentThread.Name}] TcpTextClient::OnCertificateValidate() exception={e}");
            return false;
        }
    }

    /// <summary>
    /// Give each line of the session to <see cref="OnReceiveLine" /> until the session ends.
    /// </summary>
    private async Task ReadLines(Session session)
    {
        while (true)
        {
            string line;
            try
            {
                line = await session.ReadLineAsync().ConfigureAwait(false);
            }
            catch (Exception e) when (IsIoFailure(e))
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                    $"[{Thread.CurrentThread.Name}] TcpTextClient::ReadLines() exception={e.Message}");
                LoseToIoFailure(session, e);
                return;
            }

            if (line == null)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo,
                    $"[{Thread.CurrentThread.Name}] TcpTextClient::ReadLines() remote host closed connection");
                Lose(session, DisconnectReason.RemoteHostClosedConnection);
                return;
            }

            OnReceiveLine(line);
        }
    }

    private static bool IsIoFailure(Exception e) =>
        e is IOException or SocketException or ObjectDisposedException;

    /// <summary>
    /// End <paramref name="session" /> and close it. Every path that ends a session or a
    /// connect attempt calls this.
    /// </summary>
    /// <returns>
    /// True if this call ended the session. False if the session ended before, so the caller
    /// must not report the end again.
    /// </returns>
    private bool TryEnd(Session session)
    {
        if (Interlocked.CompareExchange(ref _session, null, session) != session)
            return false;

        session.Close();
        return true;
    }

    /// <summary>
    /// End a session and call <see cref="OnDisconnect" />, if the session did not end before.
    /// </summary>
    private void Lose(Session session, DisconnectReason reason, int? socketErrorCode = null)
    {
        if (!TryEnd(session))
            return;

        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo,
            $"[{Thread.CurrentThread.Name}] TcpTextClient::Lose() reason={reason}, code={socketErrorCode}");
        OnDisconnect(reason, socketErrorCode);
    }

    /// <summary>
    /// End a session because a read or a write failed. A failure with a socket error code ends
    /// it as <see cref="DisconnectReason.SocketError" /> with that code. Any other failure, for
    /// example a stream that closed, ends it as <see cref="DisconnectReason.Other" />.
    /// </summary>
    private void LoseToIoFailure(Session session, Exception failure)
    {
        int? socketErrorCode = failure switch
        {
            SocketException socketError => socketError.ErrorCode,
            IOException { InnerException: SocketException socketError } => socketError.ErrorCode,
            _ => null
        };
        Lose(session, socketErrorCode == null ? DisconnectReason.Other : DisconnectReason.SocketError,
            socketErrorCode);
    }

    protected abstract void OnConnect();

    /// <summary>
    /// Decide whether to accept a server certificate that failed validation, for example a
    /// self-signed, expired, or wrong-host certificate.
    /// </summary>
    /// <remarks>
    /// The default rejects the certificate, and the connect fails with
    /// <see cref="ConnectError.CertificateRejected" />. Override this to trust a certificate,
    /// for example by comparing its fingerprint with one the user accepted. An exception from
    /// the override counts as a rejection.
    /// </remarks>
    /// <returns>True to accept the certificate, false to reject it.</returns>
    protected virtual bool OnCertificateValidationFailed(X509Certificate certificate, X509Chain chain,
                                                         SslPolicyErrors errors)
        => false;

    protected abstract void OnDisconnect(DisconnectReason reason, int? socketErrorCode);

    protected abstract void OnConnectFailed(ConnectError error, int? socketErrorCode);

    /// <summary>
    /// Report a failed connect attempt, with the server certificate if the certificate check
    /// rejected it.
    /// </summary>
    /// <remarks>
    /// The default calls <see cref="OnConnectFailed(ConnectError, int?)" /> and drops the
    /// certificate. Override this to get the certificate.
    /// </remarks>
    /// <param name="error">Why the connect failed.</param>
    /// <param name="socketErrorCode">The socket error code, or null if there is none.</param>
    /// <param name="rejectedCertificate">
    /// The rejected server certificate when <paramref name="error" /> is
    /// <see cref="ConnectError.CertificateRejected" /> and the server sent a certificate.
    /// Otherwise null.
    /// </param>
    protected virtual void OnConnectFailed(ConnectError error, int? socketErrorCode,
                                           RejectedCertificate rejectedCertificate)
        => OnConnectFailed(error, socketErrorCode);

    protected abstract void OnReceiveLine(string line);

    /// <summary>
    /// Why a connect attempt failed, in the form <see cref="OnConnectFailed(ConnectError, int?, RejectedCertificate)" />
    /// reports it.
    /// </summary>
    private readonly record struct ConnectFailure(ConnectError Error, int? SocketErrorCode,
                                                  RejectedCertificate RejectedCertificate = null)
    {
        /// <param name="e">The exception from the connect or the TLS handshake.</param>
        /// <param name="certificateRejected">
        /// True if the certificate check rejected the server certificate. SslStream reports a
        /// rejected certificate and other handshake failures with the same exception type, so
        /// the exception alone cannot tell them apart.
        /// </param>
        /// <param name="rejectedCertificate">
        /// A copy of the rejected certificate, or null if the server sent none.
        /// </param>
        public static ConnectFailure From(Exception e, bool certificateRejected,
                                          RejectedCertificate rejectedCertificate)
        {
            return e switch
            {
                SocketException socketError => new(ConnectError.SocketError, socketError.ErrorCode),
                AuthenticationException => certificateRejected
                    ? new(ConnectError.CertificateRejected, null, rejectedCertificate)
                    : new(ConnectError.AuthenticationFailed, null),
                // The server reset the connection during the handshake.
                IOException { InnerException: SocketException socketError } =>
                    new(ConnectError.SocketError, socketError.ErrorCode),
                // This includes an IOException with no socket error, for example when the server
                // closes the connection during the handshake.
                _ => new(ConnectError.Other, null)
            };
        }
    }

    /// <summary>
    /// One connect attempt and the connection that follows it: a socket, its stream, and the
    /// line reader on that stream. Each session has its own socket and buffers, so nothing from
    /// an ended session can get into the next one.
    /// </summary>
    /// <remarks>
    /// The session does not decide when it ends, and it does not call the hooks. The owner
    /// calls <see cref="Close" />, and reports the end.
    /// </remarks>
    private sealed class Session(Encoding encoding)
    {
        private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly byte[] _bytes = new byte[BufferLength];
        private readonly char[] _chars = new char[encoding.GetMaxCharCount(BufferLength)];
        private readonly Decoder _decoder = encoding.GetDecoder();
        private readonly StringBuilder _line = new();
        private readonly object _writeLock = new();
        private int _charIndex;
        private int _charCount;

        /// <summary>
        /// The network or TLS stream, or null until the connect and the handshake complete.
        /// </summary>
        private volatile Stream _stream;

        /// <summary>
        /// True when the connect and the handshake completed. It stays true after
        /// <see cref="Close" />, so the owner must also check that the session is current.
        /// </summary>
        public bool IsOpen => _stream != null;

        /// <summary>
        /// Connect to the server, and do the TLS handshake if <paramref name="ssl" /> is true.
        /// </summary>
        /// <param name="validate">Decides whether to accept the server certificate.</param>
        /// <returns>Null if the session is open, or why the connect failed.</returns>
        public async Task<ConnectFailure?> OpenAsync(string server, int port, bool ssl,
                                                     Func<X509Certificate, X509Chain, SslPolicyErrors, bool> validate)
        {
            var certificateRejected = false;
            RejectedCertificate rejectedCertificate = null;
            Stream stream = null;
            try
            {
                await _socket.ConnectAsync(server, port).ConfigureAwait(false);
                stream = new NetworkStream(_socket);
                if (ssl)
                {
                    var tls = new SslStream(stream, false, (_, certificate, chain, errors) =>
                    {
                        bool accepted = validate(certificate, chain, errors);
                        certificateRejected = !accepted;
                        // Copy the certificate now. SslStream disposes it after this callback.
                        if (!accepted && certificate != null)
                            rejectedCertificate = new RejectedCertificate(certificate.GetRawCertData(), errors);
                        return accepted;
                    });
                    stream = tls;
                    await tls.AuthenticateAsClientAsync(server).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                    $"[{Thread.CurrentThread.Name}] TcpTextClient::Session.OpenAsync() connect failed={e}");
                stream?.Dispose();
                return ConnectFailure.From(e, certificateRejected, rejectedCertificate);
            }

            _stream = stream;
            return null;
        }

        /// <summary>
        /// Read the next complete line, without its line end.
        /// </summary>
        /// <remarks>
        /// A "\r" is dropped wherever it occurs. A part line stays in the session until the
        /// rest of it arrives, and a part line at the end of the stream is lost.
        /// </remarks>
        /// <returns>The line, or null if the server closed the connection.</returns>
        public async Task<string> ReadLineAsync()
        {
            while (true)
            {
                while (_charIndex < _charCount)
                {
                    char item = _chars[_charIndex++];
                    if (item == '\n')
                    {
                        var line = _line.ToString();
                        _line.Clear();
                        return line;
                    }

                    if (item != '\r')
                        _line.Append(item);
                }

                int count = await _stream.ReadAsync(_bytes).ConfigureAwait(false);
                if (count == 0)
                    return null;

                _charIndex = 0;
                _charCount = _decoder.GetChars(_bytes, 0, count, _chars, 0);
            }
        }

        /// <summary>
        /// Write <paramref name="bytes" /> to the stream. Only one write runs at a time.
        /// </summary>
        public void Write(byte[] bytes)
        {
            lock (_writeLock)
                _stream.Write(bytes);
        }

        /// <summary>
        /// Close the socket and release the stream. A second call does nothing more.
        /// </summary>
        /// <remarks>
        /// The socket closes first. A <see cref="Write" /> that waits on a full send buffer
        /// then fails at once, and the stream does not release under a write that is in
        /// progress.
        /// </remarks>
        public void Close()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e) when (e is SocketException or ObjectDisposedException)
            {
                // Shutdown fails if the socket did not connect, the server reset it, or it is
                // closed already. In each case there is nothing to shut down.
            }
            _socket.Close();
            // The TLS stream holds a native security context that only Dispose releases.
            _stream?.Dispose();
        }
    }
}

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
    public bool Connected => Volatile.Read(ref _session) is { Live: true };

    /// <summary>
    /// Start a session: connect to the server, and do the TLS handshake if
    /// <paramref name="ssl" /> is true. The result comes later, through
    /// <see cref="OnConnect()" /> or <see cref="OnConnectFailed" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">A session or a connect attempt is live.</exception>
    protected void Connect(string server, int port, bool ssl)
    {
        var session = new Session(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
            TextEncoding);
        if (Interlocked.CompareExchange(ref _session, session, null) != null)
        {
            session.Socket.Dispose();
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
        if (session is not { Live: true })
            throw new InvalidOperationException("The client is not connected.");

        message += "\r\n";
        byte[] buffer = TextEncoding.GetBytes(message);
        try
        {
            lock (session.WriteLock)
                session.Stream.Write(buffer);
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
        Stream stream;
        try
        {
            stream = await OpenStream(session, server, port, ssl).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                $"[{Thread.CurrentThread.Name}] TcpTextClient::RunSession() connect failed={e}");
            (ConnectError error, int? socketErrorCode) = ClassifyConnectFailure(session, e);
            if (TryEnd(session))
                OnConnectFailed(error, socketErrorCode);
            return;
        }

        session.Stream = stream;
        session.Live = true;
        // A Disconnect during the handshake ended this session, so do not report it as connected.
        if (Volatile.Read(ref _session) != session)
            return;

        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo,
            $"[{Thread.CurrentThread.Name}] TcpTextClient::RunSession() connected to {server}, tls={ssl}");
        OnConnect();
        await ReadLines(session).ConfigureAwait(false);
    }

    private async Task<Stream> OpenStream(Session session, string server, int port, bool ssl)
    {
        await session.Socket.ConnectAsync(server, port).ConfigureAwait(false);
        var network = new NetworkStream(session.Socket);
        if (!ssl)
            return network;

        var tls = new SslStream(network, false,
            (_, certificate, chain, errors) => OnCertificateValidate(session, certificate, chain, errors));
        await tls.AuthenticateAsClientAsync(server).ConfigureAwait(false);
        return tls;
    }

    private static (ConnectError, int?) ClassifyConnectFailure(Session session, Exception e) => e switch
    {
        SocketException socketError => (ConnectError.SocketError, socketError.ErrorCode),
        // SslStream reports a rejected certificate and other handshake failures with the same
        // exception type, so the session records which one it was.
        AuthenticationException => (session.CertificateRejected
            ? ConnectError.CertificateRejected
            : ConnectError.AuthenticationFailed, null),
        // The server reset the connection during the handshake.
        IOException { InnerException: SocketException socketError } =>
            (ConnectError.SocketError, socketError.ErrorCode),
        // This includes an IOException with no socket error, for example when the server closes
        // the connection during the handshake.
        _ => (ConnectError.Other, null)
    };

    private bool OnCertificateValidate(Session session, X509Certificate certificate, X509Chain chain,
                                       SslPolicyErrors errors)
    {
        if (errors == SslPolicyErrors.None)
            return true;

        bool accepted;
        try
        {
            accepted = OnCertificateValidationFailed(certificate, chain, errors);
        }
        catch (Exception e)
        {
            // An exception from the override counts as a rejection. SslStream calls this on a
            // thread-pool thread, where an exception would end the process.
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                $"[{Thread.CurrentThread.Name}] TcpTextClient::OnCertificateValidate() exception={e}");
            accepted = false;
        }

        session.CertificateRejected = !accepted;
        return accepted;
    }

    private async Task ReadLines(Session session)
    {
        while (true)
        {
            int count;
            try
            {
                count = await session.Stream.ReadAsync(session.Bytes).ConfigureAwait(false);
            }
            catch (Exception e) when (IsIoFailure(e))
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                    $"[{Thread.CurrentThread.Name}] TcpTextClient::ReadLines() exception={e.Message}");
                LoseToIoFailure(session, e);
                return;
            }

            if (count == 0)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo,
                    $"[{Thread.CurrentThread.Name}] TcpTextClient::ReadLines() remote host closed connection");
                Lose(session, DisconnectReason.RemoteHostClosedConnection);
                return;
            }

            DispatchLines(session, count);
        }
    }

    /// <summary>
    /// Decode the bytes that a read put in the session buffer, and give each complete line to
    /// <see cref="OnReceiveLine" />. The session keeps a part line, and the decoder keeps a part
    /// character, until the next read.
    /// </summary>
    private void DispatchLines(Session session, int count)
    {
        int length = session.Decoder.GetChars(session.Bytes, 0, count, session.Chars, 0);
        for (var i = 0; i < length; i++)
        {
            char item = session.Chars[i];
            switch (item)
            {
                case '\r':
                    continue;

                case '\n':
                    OnReceiveLine(session.Line.ToString());
                    session.Line.Clear();
                    break;

                default:
                    session.Line.Append(item);
                    break;
            }
        }
    }

    private static bool IsIoFailure(Exception e) =>
        e is IOException or SocketException or ObjectDisposedException;

    /// <summary>
    /// End <paramref name="session" /> and close its socket. Every path that ends a session or
    /// a connect attempt calls this.
    /// </summary>
    /// <returns>
    /// True if this call ended the session. False if the session ended before, so the caller
    /// must not report the end again.
    /// </returns>
    private bool TryEnd(Session session)
    {
        if (Interlocked.CompareExchange(ref _session, null, session) != session)
            return false;

        session.Live = false;
        try
        {
            session.Socket.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException)
        {
            // Shutdown fails if the socket did not connect or the server reset it. In both
            // cases there is nothing to shut down, and Close below releases the socket.
        }
        session.Socket.Close();
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

    protected abstract void OnReceiveLine(string line);

    /// <summary>
    /// One connect attempt and the connection that follows it. Each session has its own socket
    /// and buffers, so nothing from an ended session can get into the next one.
    /// </summary>
    private sealed class Session(Socket socket, Encoding encoding)
    {
        public readonly Socket Socket = socket;
        public readonly byte[] Bytes = new byte[BufferLength];
        public readonly char[] Chars = new char[encoding.GetMaxCharCount(BufferLength)];
        public readonly Decoder Decoder = encoding.GetDecoder();
        public readonly StringBuilder Line = new();

        /// <summary>Lets only one <see cref="TcpTextClient.Send" /> write at a time.</summary>
        public readonly object WriteLock = new();

        /// <summary>The network or TLS stream. Set before <see cref="Live" /> becomes true.</summary>
        public Stream Stream;

        /// <summary>True from just before <see cref="TcpTextClient.OnConnect()" /> until the session ends.</summary>
        public volatile bool Live;

        public bool CertificateRejected;
    }
}

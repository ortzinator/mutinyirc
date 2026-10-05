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

namespace FlamingIRC;

public enum ConnectError
{
    ConnectionRefused,
    AuthenticationFailed,
    SocketError,
    Other
}

public enum DisconnectReason
{
    UserInitiated,
    PingTimeout,
    SocketError,
    RemoteHostClosedConnection,
    Other
}

public abstract class TcpTextClient
{
    private const int BufferLength = 512;
    private readonly byte[] _byteBuffer;
    private Socket _socket;
    private readonly StringBuilder _textBuffer;
    private string _serverName;
    private SslStream _sslStream;
    private Stream _stream;
    private bool _usesSsl;

    protected TcpTextClient()
    {
        _byteBuffer = new byte[BufferLength];
        _textBuffer = new StringBuilder();
        TextEncoding = Encoding.UTF8;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public Encoding TextEncoding { get; protected set; }

    public bool Connected { get; protected set; }

    /// <summary>
    /// Create a new connection to the server. Will call OnConnected or OnDisconnected depending
    /// on connection attempt outcome.
    /// </summary>
    /// <param name="server">A <see cref="string" /></param>
    /// <param name="port">A <see cref="int" /></param>
    /// <param name="ssl">A <see cref="bool" /></param>
    protected void Connect(string server, int port, bool ssl)
    {
        _usesSsl = ssl;
        _serverName = server;
        _socket.BeginConnect(server, port, OnConnect, null);
    }

    /// <summary>
    /// Close the connection to the server.
    /// </summary>
    /// <remarks>
    /// The client need not be actually connected. This method will also "give up" an attept to
    /// connect or revert to a disconnected state after an error.
    /// </remarks>
    public void Disconnect(DisconnectReason reason)
    {
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo,
            $"[{Thread.CurrentThread.Name}] TcpTextClient::Disconnect() reason={reason}");
        EndSession();
        OnDisconnect(reason, null);
    }

    /// <summary>
    /// End the current session or connect attempt. Every path that ends one calls this.
    /// </summary>
    /// <remarks>
    /// A socket cannot connect again after it was connected or after a connect failed, so this
    /// puts a new socket in its place. It also clears <see cref="_stream" />. An I/O callback
    /// compares its stream with <see cref="_stream" />, so a callback from an ended session
    /// cannot end the session that comes after it.
    /// </remarks>
    private void EndSession()
    {
        _stream = null;
        try { _socket.Shutdown(SocketShutdown.Both); } catch (SocketException) { }
        _socket.Close();
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Connected = false;
    }

    /// <summary>
    /// End a connect attempt that did not succeed and tell the subclass why.
    /// </summary>
    private void FailConnect(ConnectError error, int? socketErrorCode)
    {
        EndSession();
        OnConnectFailed(error, socketErrorCode);
    }

    /// <summary>
    /// End a live session because its stream failed. Do nothing if the stream belongs to a
    /// session that already ended.
    /// </summary>
    private void LoseSession(Stream stream, DisconnectReason reason)
    {
        if (stream != _stream)
            return;
        Disconnect(reason);
    }

    /// <summary>
    /// Sends a string message to the server.
    /// </summary>
    /// <param name="message">A <see cref="string" /></param>
    /// <exception cref="InvalidOperationException">The client is not connected.</exception>
    public void Send(string message)
    {
        Stream stream = _stream ?? throw new InvalidOperationException("The client is not connected.");
        message += "\r\n";
        byte[] buffer = TextEncoding.GetBytes(message);
        stream.BeginWrite(buffer, 0, buffer.Length, OnSend, stream);
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] TcpTextClient::Send() {message.TrimEnd()}");
    }

    private void OnConnect(IAsyncResult res)
    {
        try
        {
            _socket.EndConnect(res);
            _stream = new NetworkStream(_socket);

            if (_usesSsl)
            {
                _sslStream = new SslStream(_stream, false, OnCertificateValidate);
                // Set _stream first. The handshake can complete before BeginAuthenticateAsClient
                // returns, and OnAuthenticate then reads from _stream.
                _stream = _sslStream;
                _sslStream.BeginAuthenticateAsClient(_serverName, OnAuthenticate, null);
            }
            else
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo,
                    $"[{Thread.CurrentThread.Name}] TcpTextClient::OnConnect() TCP connected to {_serverName}");
                OnConnect();
                WaitForData();
            }
        }
        catch (SocketException e)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                $"[{Thread.CurrentThread.Name}] TcpTextClient::OnConnect() SocketException={e.SocketErrorCode}");
            FailConnect(ConnectError.SocketError, e.ErrorCode);
        }
        catch (Exception e)
        {
            // Do not throw again. This method is an async callback, so an exception that
            // leaves it ends the process. ConnectFailed tells the caller about the failure.
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                $"[{Thread.CurrentThread.Name}] TcpTextClient::OnConnect() exception={e}");
            FailConnect(ConnectError.SocketError, null);
        }
    }

    private bool OnCertificateValidate(object sender, X509Certificate certificate, X509Chain chain,
                                       SslPolicyErrors errors)
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

    private void OnAuthenticate(IAsyncResult res)
    {
        try
        {
            _sslStream.EndAuthenticateAsClient(res);
        }
        catch (AuthenticationException e)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                $"[{Thread.CurrentThread.Name}] TcpTextClient::OnAuthenticate() SSL authentication failed={e.Message}");
            FailConnect(ConnectError.AuthenticationFailed, null);
            return;
        }
        catch (IOException e)
        {
            // The server closed or reset the connection during the handshake.
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                $"[{Thread.CurrentThread.Name}] TcpTextClient::OnAuthenticate() handshake IO failure={e.Message}");
            FailConnect(ConnectError.SocketError, (e.InnerException as SocketException)?.ErrorCode);
            return;
        }
        catch (Exception e)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                $"[{Thread.CurrentThread.Name}] TcpTextClient::OnAuthenticate() handshake failure={e}");
            FailConnect(ConnectError.Other, null);
            return;
        }

        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo,
            $"[{Thread.CurrentThread.Name}] TcpTextClient::OnAuthenticate() SSL authenticated to {_serverName}");
        OnConnect();
        WaitForData();
    }

    // The I/O callbacks below do not throw. They run on thread-pool threads, where an exception
    // ends the process. A failure ends the session, and ConnectionLost tells the caller.

    private void WaitForData()
    {
        // The session can end while OnConnect runs, for example when the registration send fails.
        Stream stream = _stream;
        if (stream == null)
            return;

        try
        {
            stream.BeginRead(_byteBuffer, 0, BufferLength, OnDataReceived, stream);
        }
        catch (Exception e)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                $"[{Thread.CurrentThread.Name}] TcpTextClient::WaitForData() exception={e.Message}");
            LoseSession(stream, DisconnectReason.SocketError);
        }
    }

    private void OnDataReceived(IAsyncResult res)
    {
        var stream = (Stream)res.AsyncState;
        if (stream != _stream)
            return;

        try
        {
            int bytes = stream.EndRead(res);

            if (bytes == 0)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo,
                    $"[{Thread.CurrentThread.Name}] TcpTextClient::OnDataReceived() remote host closed connection");
                LoseSession(stream, DisconnectReason.RemoteHostClosedConnection);
                return;
            }

            string text =
                TextEncoding.GetString((bytes == BufferLength) ? _byteBuffer : _byteBuffer.Slice(0, bytes));
            foreach (char item in text)
            {
                switch (item)
                {
                    case '\r':
                        continue;

                    case '\n':
                        OnReceiveLine(_textBuffer.ToString());
                        _textBuffer.Clear();
                        break;

                    default:
                        _textBuffer.Append(item);
                        break;
                }
            }

            WaitForData();
        }
        catch (Exception e) when (e is IOException or SocketException or ObjectDisposedException)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                $"[{Thread.CurrentThread.Name}] TcpTextClient::OnDataReceived() exception={e.Message}");
            LoseSession(stream, DisconnectReason.SocketError);
        }
    }

    private void OnSend(IAsyncResult res)
    {
        var stream = (Stream)res.AsyncState;
        try
        {
            stream.EndWrite(res);
        }
        catch (Exception e)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                $"[{Thread.CurrentThread.Name}] TcpTextClient::OnSend() exception={e.Message}");
            LoseSession(stream, DisconnectReason.SocketError);
        }
    }

    protected abstract void OnConnect();

    /// <summary>
    /// Decide whether to accept a server certificate that failed validation, for example a
    /// self-signed, expired, or wrong-host certificate.
    /// </summary>
    /// <remarks>
    /// The default rejects the certificate, and the connect fails with
    /// <see cref="ConnectError.AuthenticationFailed" />. Override this to trust a certificate,
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
}

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

namespace FlamingIRC
{
    using System;
    using System.IO;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

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
        /// <param name="server">A <see cref="System.String" /></param>
        /// <param name="port">A <see cref="System.Int32" /></param>
        /// <param name="ssl">A <see cref="System.Boolean" /></param>
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
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Connected = false;
            OnDisconnect(reason, null);
        }

        /// <summary>
        /// Sends a string message to the server.
        /// </summary>
        /// <param name="message">A <see cref="System.String" /></param>
        public void Send(string message)
        {
            message = message + "\r\n";
            byte[] buffer = TextEncoding.GetBytes(message);
            _stream.BeginWrite(buffer, 0, buffer.Length, OnSend, null);
            Console.Write("Outgoing: {0}", message);
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
                    _sslStream.BeginAuthenticateAsClient(_serverName, OnAuthenticate, null);
                    _stream = _sslStream;
                }
                else
                {
                    OnConnect();
                    WaitForData();
                }
            }
            catch (SocketException e)
            {
                OnConnectFailed(ConnectError.SocketError, e.ErrorCode);
            }
            catch (Exception e)
            {
                OnConnectFailed(ConnectError.SocketError, null);
                System.Diagnostics.Debug.WriteLine("Connect failed: " + e.Message);
                throw;
            }
        }

        private bool OnCertificateValidate(object sender, X509Certificate certificate, X509Chain chain,
                                           SslPolicyErrors errors)
        {
            if (errors != SslPolicyErrors.None)
            {
                return OnCertificateValidatecateFailed(certificate, chain, errors);
            }

            return true;
        }

        private void OnAuthenticate(IAsyncResult res)
        {
            try
            {
                _sslStream.EndAuthenticateAsClient(res);
                OnConnect();
                WaitForData();
            }
            catch (AuthenticationException)
            {
                _socket.Shutdown(SocketShutdown.Both);
                Connected = false;
                OnConnectFailed(ConnectError.AuthenticationFailed, null);
            }
        }

        private void WaitForData()
        {
            try
            {
                _stream.BeginRead(_byteBuffer, 0, BufferLength, OnDataReceived, null);
            }
            catch (Exception)
            {
                _socket.Shutdown(SocketShutdown.Both);
                Connected = false;
                OnDisconnect(DisconnectReason.SocketError, null);
                throw; //Wasn't really handled so pass it through
            }
        }

        private void OnDataReceived(IAsyncResult res)
        {
            if (!_socket.Connected)
                return;

            try
            {
                int bytes = _stream.EndRead(res);

                if (bytes == 0)
                {
                    // Connection Closed!
                    _socket.Shutdown(SocketShutdown.Both);
                    Connected = false;
                    OnDisconnect(DisconnectReason.RemoteHostClosedConnection, null);
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
            catch (IOException)
            {
                Disconnect(DisconnectReason.SocketError);
            }
            catch (SocketException) //hack - What kind of exception?
            {
                Disconnect(DisconnectReason.SocketError);
                throw; //Wasn't really handled so pass it through
            }
        }

        private void OnSend(IAsyncResult res)
        {
            try
            {
                _stream.EndWrite(res);
            }
            catch (Exception)
            {
                _socket.Shutdown(SocketShutdown.Both);
                Connected = false;
                OnDisconnect(DisconnectReason.SocketError, null);
                throw; //Wasn't really handled so pass it through
            }
        }

        protected abstract void OnConnect();

        protected abstract bool OnCertificateValidatecateFailed(X509Certificate certificate, X509Chain chain,
                                                                SslPolicyErrors errors);

        protected abstract void OnDisconnect(DisconnectReason reason, int? socketErrorCode);

        protected abstract void OnConnectFailed(ConnectError error, int? socketErrorCode);

        protected abstract void OnReceiveLine(string line);
    }
}
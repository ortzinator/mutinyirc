/*
 * FlamingIRC IRC library
 * Copyright (C) 2002 Aaron Hunter <thresher@sharkbite.org>
 * 
 * Based on code copyright (C) 2002 Aaron Hunter <thresher@sharkbite.org>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * 
 * See the gpl.txt file located in the top-level-directory of
 * the archive of this library for complete text of license.
*/

using System;
using System.Threading;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace FlamingIRC;

/// <summary>
/// Establish a DCC Chat connection with a remote user.
/// </summary>
public sealed class DccChatSession
{

    /// <summary>
    /// The remote user did not answer the chat request in the timeout period.
    /// </summary>
    public event EventHandler<DccChatSessionEventArgs> OnChatRequestTimeout;
    /// <summary>
    /// The chat session is open. This event occurs for each session, when this client or
    /// the remote user started it.
    /// </summary>
    public event EventHandler<DccChatSessionEventArgs> OnChatSessionOpened;
    /// <summary>
    /// The chat session is closed. This client or the remote user closed it.
    /// </summary>
    public event EventHandler<DccChatSessionEventArgs> OnChatSessionClosed;
    /// <summary>
    /// The remote user sent a line of text.
    /// </summary>
    public event EventHandler<DccChatMessageEventArgs> OnChatMessageReceived;

    //Default timeout is 30 seconds
    private const int DefaultTimeout = 30000;
    private TcpClient _client;
    private TcpListener _server;
    private Thread _thread;
    private int _listenPort;
    private bool _listening;
    private bool _receiving;

    internal DccChatSession(DccUser dccUser)
    {
        ClientInfo = dccUser;
        _listening = false;
        _receiving = false;
    }

    /// <summary>
    /// A read-only property indicating whether this session
    /// is currently connected to another user.
    /// </summary>
    /// <value>True if the client is connected.</value>
    public bool Connected => _client != null;
    /// <summary>
    /// Iinformation about the remote user.
    /// </summary>
    /// <value>A read-only instance of DccUser.</value>
    public DccUser ClientInfo { get; }

    private void CloseClientConnection()
    {
        try
        {
            _client?.Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccChatSession::CloseClientConnection() " + ex);
        }
    }
    /// <summary>
    /// Send the session closed event
    /// </summary>
    private void SendClosedEvent()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::SendClosedEvent()");
        OnChatSessionClosed?.Invoke(this, new DccChatSessionEventArgs(this));
    }
    /// <summary>
    /// Create the correctly formatted DCC CHAT message and send it.
    /// </summary>
    private void SendChatRequest(string listenIPAddress, int listenPort)
    {
        //512 is the max IRC message size
        StringBuilder builder = new StringBuilder("PRIVMSG ", 512);
        builder.Append(ClientInfo.Nick);
        builder.Append(" :\x0001DCC CHAT CHAT ");
        builder.Append(DccUtil.IPAddressToLong(IPAddress.Parse(listenIPAddress)).ToString());
        builder.Append(" ");
        builder.Append(listenPort);
        builder.Append("\x0001\n");
        ClientInfo.Connection.Sender.Raw(builder.ToString());
    }
    /// <summary>
    /// Called when timeout thread is done.
    /// If the session has not yet not connected
    /// then stop listening and send a OnChatRequestTimeout
    /// event.
    /// </summary>
    /// <param name="state">An instance of DccChatSession</param>
    private void TimerExpired(object state)
    {
        if (_listening)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::TimerExpired() Chat session " + ToString() + " timed out.");
            OnChatRequestTimeout?.Invoke(this, new DccChatSessionEventArgs(this));
            Close();
        }
    }
    private void Listen()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Listen()");
        try
        {
            //Wait for remote client to connect
            IPEndPoint localEndPoint = new IPEndPoint(DccUtil.LocalHost(), _listenPort);
            _server = new TcpListener(localEndPoint);
            _listening = true;
            _server.Start();
            //Got one!
            _client = _server.AcceptTcpClient();
            _server.Stop();
            _listening = false;
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Listen() Remote user connected.");
            OnChatSessionOpened?.Invoke(this, new DccChatSessionEventArgs(this));
            //Start listening for messages
            ReceiveMessages();
        }
        catch (Exception)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Listen() Connection broken");
        }
        finally
        {
            SendClosedEvent();
        }
    }

    private void Connect()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Connect()");
        try
        {
            _client = new TcpClient();
            _client.Connect(ClientInfo.RemoteEndPoint);
            OnChatSessionOpened?.Invoke(this, new DccChatSessionEventArgs(this));
            ReceiveMessages();
        }
        catch (Exception se)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccChatSession::Connect() exception=" + se);
            if (se.Message.IndexOf("refused") > 0)
            {
                ClientInfo.Connection.Listener.Error(ReplyCode.DccConnectionRefused, "Connection refused by remote user.");
            }
            else
            {
                ClientInfo.Connection.Listener.Error(ReplyCode.ConnectionFailed, "Unknown socket error:" + se.Message);
            }
            CloseClientConnection();
        }
        finally
        {
            SendClosedEvent();
        }
    }
    private void ReceiveMessages()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::ReceiveMessages()");
        try
        {
            _receiving = true;
            string message = "";
            StreamReader reader = new StreamReader(_client.GetStream(), ClientInfo.Connection.TextEncoding);
            while ((message = reader.ReadLine()) != null)
            {
                if (OnChatMessageReceived != null)
                {
                    Debug.Indent();
                    Debug.WriteLineIf(DccUtil.DccTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] DccChatSession::ReceiveMessages() Session: " + ToString() + " Received: " + message);
                    Debug.Unindent();
                    OnChatMessageReceived?.Invoke(this, new DccChatMessageEventArgs(this, message));
                }
            }
            _receiving = false;
            //Read loop broken. Remote user must have closed the socket
            ClientInfo.Connection.Listener.Error(ReplyCode.ConnectionFailed, "Chat connection closed by remote user.");
        }
        catch (Exception ex)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccChatSession::ReceiveMessages() exception= " + ex);
        }
        finally
        {
            CloseClientConnection();
        }
    }

    /// <summary>
    /// Send a line of text to the remote user. There is no fixed
    /// limit to message size but they should not be too long.
    /// </summary>
    /// <param name="text">The text. It need not have a
    /// newline at the end since one will automatically appended..</param>
    public void SendMessage(string text)
    {
        if (Connected)
        {
            try
            {
                //Some IRC client are looking for a newline (ie mIRC) so add one
                //before sending. Also strip off any existing new lines so
                //we don't accidentally send two.
                byte[] messageBytes = ClientInfo.Connection.TextEncoding.GetBytes(text.TrimEnd() + "\n");
                _client.GetStream().Write(messageBytes, 0, messageBytes.Length);
                Debug.WriteLineIf(DccUtil.DccTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] DccChatSession::SendMessage() Sent : " + text + " Size: " + messageBytes.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLineIf(DccUtil.DccTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccChatSession::SendMessage() " + ex);
            }
        }
    }
    /// <summary>
    /// Close the chat session.
    /// </summary>
    public void Close()
    {
        //Locked because it may be called by the Timer or client thread
        lock (this)
        {
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Close()");
            if (_listening)
            {
                _server.Stop();
            }
            else if (_receiving)
            {
                //Closing the client breaks the blocking ReadLine in ReceiveMessages,
                //which then exits via the general exception/finally path.
                CloseClientConnection();
            }
        }
    }
    /// <summary>
    /// Summary information about this session.
    /// </summary>
    /// <returns>Simple information about this session in human readable format.</returns>
    public override string ToString() => "DccChatSession::" + ClientInfo.ToString();

    /// <summary>
    /// When another a remote user has sent a chat request, this
    /// method is called to accept the request and
    /// start a chat session with that user.
    /// </summary>
    /// <remarks>
    /// This method should be called from within a try/catch block
    /// because there are many things that could prevent this
    /// connection attempt from succeeding.
    /// </remarks>
    /// <param name="dccUser">A collection of information about the remote user.</param>
    /// <returns>The DccChatSession instance for this session.</returns>
    public static DccChatSession Accept(DccUser dccUser)
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Accept()");
        DccChatSession session = new DccChatSession(dccUser);
        //Start session Thread
        session._thread = new Thread(new ThreadStart(session.Connect))
        {
            Name = session.ToString()
        };
        session._thread.Start();
        return session;
    }
    /// <summary>
    /// Send a DCC Chat request to a remote user and use the default
    /// timeout period of 30 seconds.
    /// </summary>
    /// <remarks>
    /// <para>If the user does not respond within the timeout period the DccChatSession
    /// will stop listening for a connection. The sesssion instance created then becomes
    /// invalid. This methods must be called again and a new instance created in order to
    /// initiate a try again.
    /// </para>
    /// <para>
    /// This method should be called from within a try/catch block
    /// in case there are socket errors.
    /// </para>
    /// </remarks>
    /// <param name="dccUser">A collection of information about the remote user.</param>
    /// <param name="listenIPAddress">The IP address of the local machine in dot
    /// quad format (e.g. 192.168.0.25). This is the address that will be sent to the
    /// remote user. The IP address of the NAT machine must be used if the
    /// client is behind a a NAT/Firewall system. </param>
    /// <param name="listenPort">The TCP/IP port to listen on</param>
    public static DccChatSession Request(DccUser dccUser, string listenIPAddress, int listenPort)
        => Request(dccUser, listenIPAddress, listenPort, DefaultTimeout);
    /// <summary>
    /// Send a DCC Chat request to a remote user and wait for a connection
    /// using timeout period specified.
    /// </summary>
    /// <remarks>
    /// <para>If the user does not respond within the timeout period the DccChatSession
    /// will stop listening for a connection. The sesssion instance created then becomes
    /// invalid. This methods must be called again and a new instance created in order to
    /// initiate a try again.
    /// </para>
    /// <para>
    /// This method should be called from within a try/catch block
    /// in case there are socket errors.
    /// </para>
    /// </remarks>
    /// <param name="dccUser">A collection of information about the remote user.</param>
    /// <param name="listenIPAddress">The IP address that will be sent to the remote user. It must
    /// be in dotted quad format (i.e. 192.168.0.2). If the client is behind a NAT system then
    /// this should be the address of that system and not the local host.</param>
    /// <param name="listenPort">The TCP/IP port to listen on</param>
    /// <param name="timeout">How long to wait for a response in milliseconds.
    /// A value of zero will disable the timeout.</param>
    public static DccChatSession Request(DccUser dccUser, string listenIPAddress, int listenPort, long timeout)
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Request()");
        //Create session object
        DccChatSession session = new DccChatSession(dccUser)
        {
            _listenPort = listenPort
        };
        //Start session Thread
        session._thread = new Thread(new ThreadStart(session.Listen))
        {
            Name = session.ToString()
        };
        session._thread.Start();
        //Send Chat request to remote user
        session.SendChatRequest(listenIPAddress, listenPort);
        //Start timeout thread if timeout > 0
        if (timeout > 0)
        {
            Timer timer = new Timer(
                new TimerCallback(session.TimerExpired),
                session,
                timeout,
                0);
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccChatSession::Request timeout thread started");
        }
        return session;
    }

}

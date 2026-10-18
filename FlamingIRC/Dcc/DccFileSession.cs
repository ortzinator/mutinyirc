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
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace FlamingIRC;

/// <summary>
/// Allows the user to send and receive files
/// from other IRC users.
/// </summary>
public sealed class DccFileSession
{
    /// <summary>
    /// The session had no activity for the timeout period. The session closes.
    /// </summary>
    public event EventHandler<DccFileSessionEventArgs> OnFileTransferTimeout;
    /// <summary>
    /// The file transfer connection is open, and the transfer of data starts.
    /// </summary>
    public event EventHandler<DccFileSessionEventArgs> OnFileTransferStarted;
    /// <summary>
    /// The file transfer stopped before it completed. Usually, this client or the remote
    /// user cancelled the transfer.
    /// </summary>
    public event EventHandler<DccFileSessionEventArgs> OnFileTransferInterrupted;
    /// <summary>
    /// The file transfer completed successfully.
    /// </summary>
    public event EventHandler<DccFileSessionEventArgs> OnFileTransferCompleted;
    /// <summary>
    /// The session sent or received one block of data. Use this event to show the progress
    /// of the transfer.
    /// </summary>
    public event EventHandler<DccFileProgressEventArgs> OnFileTransferProgress;

    //Does this session use send-ahead mode
    private bool _turboMode;

    //The last time any data was received or sent successfully
    //used to test for a timeout.
    //Signals whether the session is waiting for an Accept message 
    //in reponse to a Resume request.
    private bool _waitingOnAccept;
    private readonly byte[] _buffer;
    private readonly int _listenPort;
    private Socket _socket;
    private Socket _serverSocket;
    private Thread _thread;

    internal DccFileInfo dccFileInfo;

    /// <summary>
    /// Prepare a new instance with default values but do not connect
    /// to another user.
    /// </summary>
    internal DccFileSession(DccUser dccUser, DccFileInfo dccFileInfo, int bufferSize, int listenPort, string sessionID)
    {
        User = dccUser;
        this.dccFileInfo = dccFileInfo;
        _buffer = new byte[bufferSize];
        _listenPort = listenPort;
        ID = sessionID;
        LastActivity = DateTime.Now;
        _waitingOnAccept = false;
    }

    internal DateTime LastActivity { get; private set; }

    /// <summary>
    /// A unique identifier for this session.
    /// </summary>
    /// <value>Uses the TCP/IP port prefixed by an 'S' if this
    /// session is serving the file or a 'C' if this session is receiving the
    /// file.</value>
    public string ID { get; }
    /// <summary>
    /// The DccUser object associated with this DccFileSession.
    /// </summary>
    public DccUser User { get; }
    /// <summary>
    /// The DccFileInfo object associated with this DccFileSession.
    /// </summary>
    public DccFileInfo File => dccFileInfo;
    /// <summary>
    /// The information about the remote user.
    /// </summary>
    /// <value>A read only instance of DccUser.</value>
    public DccUser ClientInfo => User;

    private void SendAccept()
    {
        StringBuilder builder = new StringBuilder("PRIVMSG ", 512);
        builder.Append(User.Nick);
        builder.Append(" :\x0001DCC ACCEPT ");
        builder.Append(dccFileInfo.DccFileName);
        builder.Append(" ");
        builder.Append(_listenPort);
        builder.Append(" ");
        builder.Append(dccFileInfo.FileStartingPosition);
        builder.Append("\x0001\n");
        User.Connection.Sender.Raw(builder.ToString());
    }
    private void DccSend(IPAddress sendAddress)
    {
        StringBuilder builder = new StringBuilder("PRIVMSG ", 512);
        builder.Append(User.Nick);
        builder.Append(" :\x0001DCC SEND ");
        builder.Append(dccFileInfo.DccFileName);
        builder.Append(" ");
        builder.Append(DccUtil.IPAddressToLong(sendAddress).ToString());
        builder.Append(" ");
        builder.Append(_listenPort);
        builder.Append(" ");
        builder.Append(dccFileInfo.CompleteFileSize);
        builder.Append(_turboMode ? " T" : "");
        builder.Append("\x0001\n");
        User.Connection.Sender.Raw(builder.ToString());
    }
    private void SendResume()
    {
        StringBuilder builder = new StringBuilder("PRIVMSG ", 512);
        builder.Append(User.Nick);
        builder.Append(" :\x0001DCC RESUME ");
        builder.Append(dccFileInfo.DccFileName);
        builder.Append(" ");
        builder.Append(_listenPort);
        builder.Append(" ");
        builder.Append(dccFileInfo.FileStartingPosition);
        builder.Append("\x0001\n");
        User.Connection.Sender.Raw(builder.ToString());
    }
    /// <summary>
    /// Attempt to shut the session down correctly.
    /// </summary>
    private void Cleanup()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Cleanup()");
        DccFileSessionManager.DefaultInstance.RemoveSession(this);
        _serverSocket?.Close();
        if (_socket != null)
        {
            try
            {
                _socket.Close();
            }
            catch (Exception)
            {
                //Ignore this exception
            }
        }
        dccFileInfo.CloseFile();
    }
    private void ResetActivityTimer() => LastActivity = DateTime.Now;
    private void SignalTransferStart()
    {
        ResetActivityTimer();
        OnFileTransferStarted?.Invoke(this, new DccFileSessionEventArgs(this));
    }
    private void Listen()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Listen()");
        try
        {
            //Wait for remote client to connect
            IPEndPoint localEndPoint = new IPEndPoint(DccUtil.LocalHost(), _listenPort);
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(localEndPoint);
            _serverSocket.Listen(1);
            //Got one!
            _socket = _serverSocket.Accept();
            _serverSocket.Close();
            Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Listen() Remote user connected.");
            //Advance to the correct point in the file in case this is a resume 
            dccFileInfo.GotoReadPosition();
            SignalTransferStart();
            if (_turboMode)
            {
                Upload();
            }
            else
            {
                UploadLegacy();
            }
        }
        catch (Exception)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccFileSession::Listen() Connection broken");
            Interrupted();
        }
    }
    private void Upload()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Upload()" + (_turboMode ? " Turbo" : " Legacy") + " mode");
        try
        {
            int bytesRead = 0;
            byte[] ack = new byte[4];
            while ((bytesRead = dccFileInfo.TransferStream.Read(_buffer, 0, _buffer.Length)) != 0)
            {
                _socket.Send(_buffer, 0, bytesRead, SocketFlags.None);
                ResetActivityTimer();
                AddBytesProcessed(bytesRead);
            }
            //Now we are done
            Finished();
        }
        catch (Exception ex)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccFileSession::Upload() exception=" + ex);
            Interrupted();
        }
    }
    private void UploadLegacy()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::UploadLegacy()");
        try
        {
            int bytesRead = 0;
            byte[] ack = new byte[4];
            while ((bytesRead = dccFileInfo.TransferStream.Read(_buffer, 0, _buffer.Length)) != 0)
            {
                _socket.Send(_buffer, 0, bytesRead, SocketFlags.None);
                ResetActivityTimer();
                AddBytesProcessed(bytesRead);
                //Wait for acks from client
                _socket.Receive(ack);
            }
            //Some IRC clients need a moment to catch up on their acks if our send buffer
            //is larger than their receive buffer. Test to make sure they ack all the bytes
            //before closing. This is only needed in legacy mode.
            while (!dccFileInfo.AcksFinished(DccUtil.DccBytesToLong(ack)))
            {
                _socket.Receive(ack);
            }
            //Now we are done
            Finished();
        }
        catch (Exception ex)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccFileSession::UploadLegacy() exception=" + ex);
            Interrupted();
        }
    }
    private void Download()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Download()" + (_turboMode ? " Turbo" : " Legacy") + " mode");
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(User.RemoteEndPoint);
            int bytesRead = 0;
            while (!dccFileInfo.AllBytesTransfered())
            {
                bytesRead = _socket.Receive(_buffer);
                //Remote server closed the connection before all bytes were sent
                if (bytesRead == 0)
                {
                    Interrupted();
                    return;
                }
                ResetActivityTimer();
                AddBytesProcessed(bytesRead);
                dccFileInfo.TransferStream.Write(_buffer, 0, bytesRead);
                //Send ack if in legacy mode
                if (!_turboMode)
                {
                    _socket.Send(DccUtil.DccBytesReceivedFormat(dccFileInfo.CurrentFilePosition()));
                }
            }
            dccFileInfo.TransferStream.Flush();
            Finished();
        }
        catch (Exception ex)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] DccFileSession::Download() exception=" + ex);
            if (ex.Message.IndexOf("refused") > 0)
            {
                User.Connection.Listener.Error(ReplyCode.DccConnectionRefused, "Connection refused by remote user.");
            }
            else
            {
                User.Connection.Listener.Error(ReplyCode.ConnectionFailed, "Unknown socket error:" + ex.Message);
            }
            Interrupted();
        }
    }

    internal void AddBytesProcessed(int bytesRead)
    {
        dccFileInfo.AddBytesTransfered(bytesRead);
        OnFileTransferProgress?.Invoke(this, new DccFileProgressEventArgs(this, bytesRead));
    }
    /// <summary>
    /// Called by DccListener when it receives a DCC Accept message.
    /// </summary>
    internal void OnDccAcceptReceived(long position)
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::OnDccAcceptReceived()");
        lock (this)
        {
            //Are we still waiting on the accept?
            if (!_waitingOnAccept)
            {
                //Assume that a normal receive has gone ahead
                return;
            }
            //No longer waiting
            _waitingOnAccept = false;
            if (!dccFileInfo.AcceptPositionMatches(position))
            {
                User.Connection.Listener.Error(ReplyCode.BadDccAcceptValue, "Asked to start at " + dccFileInfo.FileStartingPosition + " but was sent " + position);
                Interrupted();
                return;
            }
            ResetActivityTimer();
            dccFileInfo.SetResumeToFileSize();
            dccFileInfo.GotoWritePosition();
            _thread = new Thread(new ThreadStart(Download))
            {
                Name = ToString()
            };
            _thread.Start();
        }
    }
    /// <summary>
    /// A DCC Send request has already been sent and the remote user
    /// has responded with a Resume request.
    /// </summary>
    /// <param name="resumePosition">The number of bytes the remote user already has..</param>
    /// <exception cref="ArgumentException">If the session is no longer active or the file
    /// resume position was larger than the file.</exception>
    internal void OnDccResumeRequest(long resumePosition)
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::OnDccResumeRequest()");
        lock (this)
        {
            ResetActivityTimer();
            //Make sure we have not already started transfering data and that this file is
            //resumeable.
            if (dccFileInfo.BytesTransfered == 0 && dccFileInfo.CanResume())
            {
                //Make sure the position is valid
                if (dccFileInfo.ResumePositionValid(resumePosition))
                {
                    dccFileInfo.SetResumePosition(resumePosition);
                    SendAccept();
                }
                else
                {
                    User.Connection.Listener.Error(ReplyCode.BadResumePosition, ToString() + " sent an invalid resume position.");
                    //Close the socket and stop listening
                    Cleanup();
                }
            }
        }
    }
    /// <summary>
    /// Called when there has been no activity is
    /// a session for the the length of the timeout period.
    /// </summary>
    internal void TimedOut()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, ToString() + " timed out.");
        if (_waitingOnAccept)
        {
            _waitingOnAccept = false;
            //Start a new thread to download the whole file
            _thread = new Thread(new ThreadStart(Download))
            {
                Name = ToString()
            };
            _thread.Start();
        }
        else
        {
            OnFileTransferTimeout?.Invoke(this, new DccFileSessionEventArgs(this));
            Cleanup();
        }
    }
    /// <summary>
    /// Non synchro version of Stop() for internal
    /// use.
    /// </summary>
    internal void Interrupted()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Interrupted()");
        Cleanup();
        OnFileTransferInterrupted?.Invoke(this, new DccFileSessionEventArgs(this));
    }
    /// <summary>
    /// The file transfer is done. So close everything
    /// cleanly.
    /// </summary>
    internal void Finished()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Finished()");
        Cleanup();
        OnFileTransferCompleted?.Invoke(this, new DccFileSessionEventArgs(this));
    }

    /// <summary>
    /// Stop the file transfer.
    /// </summary>
    public void Stop()
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Stop()");
        lock (this)
        {
            Cleanup();
            OnFileTransferInterrupted?.Invoke(this, new DccFileSessionEventArgs(this));
        }
    }
    /// <summary>
    /// Summary information about this session.
    /// </summary>
    /// <returns>Simple information about this session in human readable format.</returns>
    public override string ToString() => "DccFileSession:: ID=" + ID + " User=" + User.ToString() +
        " File=" + dccFileInfo.DccFileName;

    /// <summary>
    /// Ask a remote user to send a file. The remote user may or may not respond
    /// and there is no fixed time within which he must respond. A response will
    /// come in the form of a DCC Send request.
    /// </summary>
    /// <param name="connection">The connection the remotes user is on.</param>
    /// <param name="nick">Who to send the request to.</param>
    /// <param name="fileName">The name of the file to have sent. This should
    /// not contain any spaces.</param>
    /// <param name="turbo">True to use send-ahead mode for transfers.</param>
    public static void Get(Connection connection, string nick, string fileName, bool turbo)
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Get()");
        StringBuilder builder = new StringBuilder("PRIVMSG ", 512);
        builder.Append(nick);
        builder.Append(" :\x0001DCC GET ");
        builder.Append(fileName);
        builder.Append(turbo ? " T" : "");
        builder.Append("\x0001\n");
        connection.Sender.Raw(builder.ToString());
    }
    /// <summary>
    /// Attempt to send a file to a remote user. Start listening
    /// on the given port and address. If the remote user does not accept
    /// the offer within the timeout period the the session
    /// will be closed.
    /// </summary>
    /// <remarks>
    /// This method should be called from within a try/catch block
    /// in case there are socket errors. This methods will also automatically
    /// handle a Resume if the remote client requests it.
    /// </remarks>
    /// <param name="dccUser">The information about the remote user.</param>
    /// <param name="listenIPAddress">The IP address of the local machine in dot
    /// quad format (e.g. 192.168.0.25). This is the address that will be sent to the
    /// remote user. The IP address of the NAT machine must be used if the
    /// client is behind a NAT/Firewall system. </param>
    /// <param name="listenPort">The port that the session will listen on.</param>
    /// <param name="dccFileInfo">The file to be sent. If the file name has spaces in it
    /// they will be replaced with underscores when the name is sent.</param>
    /// <param name="bufferSize">The size of the send buffer. Generally should
    /// be between 4k and 32k.</param>
    /// <param name="turbo">True to use send-ahead mode for transfers.</param>
    /// <returns>A unique session instance for this file and remote user.</returns>
    /// <exception cref="ArgumentException">If the listen port is already in use.</exception>
    public static DccFileSession Send(
        DccUser dccUser,
        string listenIPAddress,
        int listenPort,
        DccFileInfo dccFileInfo,
        int bufferSize,
        bool turbo)
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Send()");
        DccFileSession session = null;
        //Test if we are already using this port
        if (DccFileSessionManager.DefaultInstance.ContainsSession("S" + listenPort))
        {
            throw new ArgumentException("Already listening on port " + listenPort);
        }
        try
        {
            session = new DccFileSession(dccUser, dccFileInfo, bufferSize, listenPort, "S" + listenPort)
            {
                //set turbo mode
                _turboMode = turbo
            };
            //Add session to active sessions hashtable
            DccFileSessionManager.DefaultInstance.AddSession(session);
            //Create stream to file
            dccFileInfo.OpenForRead();
            //Start session Thread
            session._thread = new Thread(new ThreadStart(session.Listen))
            {
                Name = session.ToString()
            };
            session._thread.Start();
            //Send DCC Send request to remote user
            session.DccSend(IPAddress.Parse(listenIPAddress));
            return session;
        }
        catch (Exception ex)
        {
            if (session != null)
            {
                DccFileSessionManager.DefaultInstance.RemoveSession(session);
            }
            throw;
        }
    }
    /// <summary>
    /// Another user has offered to send a file. This method should be called
    /// to accept the offer and save the file to the give location. The parameters
    /// needed to call this method are provided by the <c>OnDccFileTransferRequest()</c>
    /// event.
    /// </summary>
    /// <remarks>
    /// This method should be called from within a try/catch block
    /// in case it is unable to connect or there are other socket
    /// errors.
    /// </remarks>
    /// <param name="dccUser">Information on the remote user.</param>
    /// <param name="dccFileInfo">The local file that will hold the data being sent. If the file
    /// is the result of a previous incomplete download the the attempt will be made
    /// to resume where the previous left off.</param>
    /// <param name="turbo">Will the send ahead protocol be used.</param>
    /// <returns>A unique session instance for this file and remote user.</returns>
    /// <exception cref="ArgumentException">If the listen port is already in use.</exception>
    public static DccFileSession Receive(DccUser dccUser, DccFileInfo dccFileInfo, bool turbo)
    {
        Debug.WriteLineIf(DccUtil.DccTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] DccFileSession::Receive()");
        //Test if we are already using this port
        if (DccFileSessionManager.DefaultInstance.ContainsSession("C" + dccUser.remoteEndPoint.Port))
        {
            throw new ArgumentException("Already listening on port " + dccUser.remoteEndPoint.Port);
        }
        DccFileSession session = null;
        try
        {
            session = new DccFileSession(dccUser, dccFileInfo, 64 * 1024,
                dccUser.remoteEndPoint.Port, "C" + dccUser.remoteEndPoint.Port)
            {
                //Has the initiator specified the turbo protocol? 
                _turboMode = turbo
            };
            //Open file for writing
            dccFileInfo.OpenForWrite();
            DccFileSessionManager.DefaultInstance.AddSession(session);
            //Determine if we can resume a download
            if (session.dccFileInfo.ShouldResume())
            {
                session._waitingOnAccept = true;
                session.dccFileInfo.SetResumeToFileSize();
                session.SendResume();
            }
            else
            {
                session._thread = new Thread(new ThreadStart(session.Download))
                {
                    Name = session.ToString()
                };
                session._thread.Start();
            }
            return session;
        }
        catch (Exception ex)
        {
            if (session != null)
            {
                DccFileSessionManager.DefaultInstance.RemoveSession(session);
            }
            throw;
        }
    }

}

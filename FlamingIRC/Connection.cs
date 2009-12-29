/*
 * FlamingIRC IRC library
 * Copyright (C) 2008 Brian Ortiz & Max Schmeling <http://code.google.com/p/ortzirc>
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

using System.Net.Security;

namespace FlamingIRC
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// This class manages the connection to the IRC server and provides
    /// access to all the objects needed to send and receive messages.
    /// </summary>
    public sealed class Connection : TcpTextClient
    {
        /// <summary>
        /// Receive all the messages, unparsed, sent by the IRC server. This is not
        /// normally needed but provided for those who are interested.
        /// </summary>
        public event EventHandler<FlamingDataEventArgs<string>> RawMessageReceived;
        /// <summary>
        /// Receive all the raw messages sent to the IRC from this connection
        /// </summary>
        public event EventHandler<FlamingDataEventArgs<string>> RawMessageSent;

        public event EventHandler ConnectionEstablished;
        public event EventHandler<FlamingDataEventArgs<int>> ConnectFailed;
        public event EventHandler<FlamingDataEventArgs<string>> ConnectionLost;

        private Regex propertiesRegex;
        private readonly Listener listener;
        private readonly Sender sender;
        private CtcpListener ctcpListener;
        private CtcpSender ctcpSender;
        private CtcpResponder ctcpResponder;
        private bool ctcpEnabled;
        private bool dccEnabled;
        private DateTime timeLastSent;
        //Connected and registered with IRC server
        private bool registered;
        //TCP/IP connection established with IRC server
        private bool handleNickFailure;
        private ArrayList parsers;
        private ServerProperties properties;

        internal ConnectionArgs connectionArgs;

        /// <summary>
        /// Used for internal test purposes only.
        /// </summary>
        internal Connection(ConnectionArgs args)
        {
            connectionArgs = args;
            sender = new Sender(this);
            listener = new Listener();
            timeLastSent = DateTime.Now;
            EnableCtcp = true;
            EnableDcc = true;
            TextEncoding = Encoding.Default;
        }
        /// <summary>
        /// Prepare a connection to an IRC server but do not open it. This sets the text Encoding to Default.
        /// </summary>
        /// <param name="args">The set of information need to connect to an IRC server</param>
        /// <param name="enableCtcp">True if this Connection should support CTCP.</param>
        /// <param name="enableDcc">True if this Connection should support DCC.</param>
        public Connection(ConnectionArgs args, bool enableCtcp, bool enableDcc)
        {
            propertiesRegex = new Regex("([A-Z]+)=([^\\s]+)", RegexOptions.Compiled | RegexOptions.Singleline);
            registered = false;
            handleNickFailure = true;
            connectionArgs = args;
            parsers = new ArrayList();
            sender = new Sender(this);
            listener = new Listener();
            RegisterDelegates();
            timeLastSent = DateTime.Now;
            EnableCtcp = enableCtcp;
            EnableDcc = enableDcc;
            TextEncoding = Encoding.Default;
        }


        /// <summary>
        /// Prepare a connection to an IRC server but do not open it.
        /// </summary>
        /// <param name="args">The set of information need to connect to an IRC server</param>
        /// <param name="enableCtcp">True if this Connection should support CTCP.</param>
        /// <param name="enableDcc">True if this Connection should support DCC.</param>
        /// <param name="textEncoding">The text encoding for the incoming stream.</param>
        public Connection(Encoding textEncoding, ConnectionArgs args, bool enableCtcp, bool enableDcc)
            : this(args, enableCtcp, enableDcc)
        {
            TextEncoding = textEncoding;
        }

        /// <summary>
        /// A read-only property indicating whether the connection 
        /// has been opened with the IRC server and the 
        /// socket has been successfully registered.
        /// </summary>
        /// <value>True if the socket is connected and registered.</value>
        public bool Registered
        {
            get
            {
                return registered;
            }
        }

        /// <summary>
        /// By default the connection itself will handle the case
        /// where, while attempting to register the socket's nick
        /// is already in use. It does this by simply appending
        /// 2 random numbers to the end of the nick. 
        /// </summary>
        /// <remarks>
        /// The NickError event is shows that the nick collision has happened
        /// and it is fixed by calling Sender's Register() method passing
        /// in the replacement nickname.
        /// </remarks>
        /// <value>True if the connection should handle this case and
        /// false if the socket will handle it itself.</value>
        public bool HandleNickTaken
        {
            get
            {
                return handleNickFailure;
            }
            set
            {
                handleNickFailure = value;
            }
        }

        /// <summary>
        /// A user friendly name of this Connection in the form 'nick@host'
        /// </summary>
        /// <value>Read only string</value>
        public string Name
        {
            get
            {
                return connectionArgs.Nick + "@" + connectionArgs.Hostname;
            }
        }

        /// <summary>
        /// Whether Ctcp commands should be processed and if
        /// Ctcp events will be raised.
        /// </summary>
        /// <value>True will enable the CTCP sender and listener and
        /// false will cause their property calls to return null.</value>
        public bool EnableCtcp
        {
            get
            {
                return ctcpEnabled;
            }
            set
            {
                if (value && !ctcpEnabled)
                {
                    ctcpListener = new CtcpListener(this);
                    ctcpSender = new CtcpSender(this);
                }
                else if (!value)
                {
                    ctcpListener = null;
                    ctcpSender = null;
                }
                ctcpEnabled = value;
            }
        }

        /// <summary>
        /// Whether DCC requests should be processed or ignored 
        /// by this Connection. Since the DccListener is a singleton and
        /// shared by all Connections, listeners to DccListener events should
        /// be manually removed when no longer needed.
        /// </summary>
        /// <value>True to process DCC requests.</value>
        public bool EnableDcc
        {
            get
            {
                return dccEnabled;
            }
            set
            {
                dccEnabled = value;
            }
        }

        /// <summary>
        /// Sets an automatic responder to Ctcp queries.
        /// </summary>
        /// <value>Once this is set it can be removed by setting it to null.</value>
        public CtcpResponder CtcpResponder
        {
            get
            {
                return ctcpResponder;
            }
            set
            {
                if (value == null && ctcpResponder != null)
                {
                    ctcpResponder.Disable();
                }
                ctcpResponder = value;
            }
        }

        /// <summary>
        /// The amount of time that has passed since the socket
        /// sent a command to the IRC server.
        /// </summary>
        /// <value>Read only TimeSpan</value>
        public TimeSpan IdleTime
        {
            get
            {
                return DateTime.Now - timeLastSent;
            }
        }

        /// <summary>
        /// The object used to send commands to the IRC server.
        /// </summary>
        /// <value>Read-only Sender.</value>
        public Sender Sender
        {
            get
            {
                return sender;
            }
        }

        /// <summary>
        /// The object that parses messages and notifies the appropriate delegate.
        /// </summary>
        /// <value>Read only Listener.</value>
        public Listener Listener
        {
            get
            {
                return listener;
            }
        }

        /// <summary>
        /// The object used to send CTCP commands to the IRC server.
        /// </summary>
        /// <value>Read only CtcpSender. Null if CtcpEnabled is false.</value>
        public CtcpSender CtcpSender
        {
            get
            {
                return ctcpSender;
            }
        }

        /// <summary>
        /// The object that parses CTCP messages and notifies the appropriate delegate.
        /// </summary>
        /// <value>Read only CtcpListener. Null if CtcpEnabled is false.</value>
        public CtcpListener CtcpListener
        {
            get { return ctcpEnabled ? ctcpListener : null; }
        }

        /// <summary>
        /// The collection of data used to establish this connection.
        /// </summary>
        /// <value>Read only ConnectionArgs.</value>
        public ConnectionArgs ConnectionData
        {
            get
            {
                return connectionArgs;
            }
        }

        /// <summary>
        /// A read-only collection of string key/value pairs
        /// representing IRC server proprties.
        /// </summary>
        /// <value>This connection's ServerProperties object is null if it 
        /// has not been created.</value>
        public ServerProperties ServerProperties
        {
            get
            {
                return properties;
            }
        }

        private bool CustomParse(string line)
        {
            foreach (IParser parser in parsers)
            {
                if (parser.CanParse(line))
                {
                    parser.Parse(line);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Respond to IRC keep-alives.
        /// </summary>
        /// <param name="message">The message that should be echoed back</param>
        private void KeepAlive(string message)
        {
            sender.Pong(message);
        }

        /// <summary>
        /// Update the ConnectionArgs object when the user
        /// changes his nick.
        /// </summary>
        /// <param name="user">Who changed their nick</param>
        /// <param name="newNick">The new nick name</param>
        private void MyNickChanged(User user, string newNick)
        {
            if (connectionArgs.Nick == user.Nick)
                connectionArgs.Nick = newNick;
        }

        private void OnRegistered(object sender, EventArgs e)
        {
            registered = true;
            listener.OnRegistered -= OnRegistered;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnNickError(object sender, NickErrorEventArgs ea)
        {
            //If this is our initial connection attempt
            if (!registered && handleNickFailure)
            {
                var generator = new NameGenerator();
                string nick;
                do
                {
                    nick = generator.MakeName();
                }
                while (!Rfc2812Util.IsValidNick(nick) || nick.Length == 1);
                //Try to reconnect
                Sender.Register(nick);
            }
        }
        /// <summary>
        /// Listen for the 005 info messages sent during registration so that the maximum lengths
        /// of certain items (Nick, Away, Topic) can be determined dynamically.
        /// </summary>
        private void OnReply(object sender, ReplyEventArgs a)
        {
            if (a.ReplyCode != ReplyCode.RPL_BOUNCE) return;

            //Lazy instantiation
            if (properties == null)
            {
                properties = new ServerProperties();
            }
            //Populate properties from name/value matches
            MatchCollection matches = propertiesRegex.Matches(a.Message);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    properties.SetProperty(match.Groups[1].ToString(), match.Groups[2].ToString());
                }
            }
            //Extract ones we are interested in
            ExtractProperties();
        }
        private void ExtractProperties()
        {
            //For the moment the only one we care about is NickLen
            //In fact we don't cae about any but keep here as an example
            /*
            if( properties.ContainsKey("NICKLEN") ) 
            {
                try 
                {
                    maxNickLength = int.Parse( properties[ "NICKLEN" ] );
                }
                catch( Exception e )
                {
                }
            }
            */
        }
        private void RegisterDelegates()
        {
            listener.OnPing += KeepAlive;
            listener.OnNick += MyNickChanged;
            listener.OnNickError += OnNickError;
            listener.OnReply += OnReply;
            listener.OnRegistered += OnRegistered;
        }

        /// <summary>
        /// Connect to the IRC server and start listening for messages asynchronously
        /// </summary>
        public void Connect()
        {
            lock (this)
            {
                if (Connected)
                    throw new Exception("Connection with IRC server already opened.");

                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] Connection::Connect()");

                Connect(connectionArgs.Hostname, connectionArgs.Port, connectionArgs.Ssl);
            }
        }


        /// <summary>
        /// Read in message lines from the IRC server
        /// and send them to a parser for processing.
        /// Discard CTCP and DCC messages if these protocols
        /// are not enabled.
        /// </summary>
        internal void ReceiveIRCMessages(string line)
        {
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] Connection::ReceiveIRCMessages()");

            try
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] Connection::ReceiveIRCMessages() rec'd:" + line);
                //Try any custom parsers first
                if (CustomParse(line))
                {
                    //One of the custom parsers handled this message so
                    //we go back to listening
                    return;
                }
                if (DccListener.IsDccRequest(line))
                {
                    if (dccEnabled)
                    {
                        DccListener.DefaultInstance.Parse(this, line);
                    }
                }
                else if (CtcpListener.IsCtcpMessage(line))
                {
                    if (ctcpEnabled)
                    {
                        ctcpListener.Parse(line);
                    }
                }
                else
                {
                    listener.Parse(line);
                }

                RawMessageReceived.Fire(this, new FlamingDataEventArgs<string>(line));
            }
            catch (IOException e)
            {
                //Trap a connection failure
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Connection::ReceiveIRCMessages() IO Error while listening for messages " + e);
                listener.Error(ReplyCode.ConnectionFailed, "Connection to server unexpectedly failed.");
            }
        }

        /// <summary>
        /// Send a message to the IRC server and clear the command buffer.
        /// </summary>
        internal void SendCommand(StringBuilder command)
        {
            try
            {
                Send(command.ToString());

                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] Connection::SendCommand() sent= " + command);
                timeLastSent = DateTime.Now;
            }
            catch (Exception ex)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Connection::SendCommand() exception=" + ex);
            }

            RawMessageSent.Fire(this, new FlamingDataEventArgs<string>(command.ToString()));
            Trace.WriteLine("Sent command: " + command, "IRC");

            command.Remove(0, command.Length);
        }

        /// <summary>
        /// Send a message to the IRC server which does
        /// not affect the socket's idle time. Used for automatic replies
        /// such as PONG or Ctcp repsones.
        /// </summary>
        internal void SendAutomaticReply(StringBuilder command)
        {
            try
            {
                Send(command.ToString());

                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose, "[" + Thread.CurrentThread.Name + "] Connection::SendAutomaticReply() message=" + command);
            }
            catch (Exception ex)
            {
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Connection::SendAutomaticReply() exception=" + ex);
            }
            command.Remove(0, command.Length);
        }

        /// <summary>
        /// Sends a 'Quit' message to the server, and closes the connection.
        /// </summary>
        /// <remarks>The state of the connection will remain the same even after a disconnect,
        /// so the connection can be reopened. All the event handlers will remain registered.</remarks>
        /// <param name="reason">A message displayed to other IRC users upon disconnect.</param>
        public void Disconnect(string reason)
        {
            lock (this)
            {
                if (!Connected)
                    return;

                listener.Disconnecting();
                sender.Quit(reason);
                Disconnect();
                listener.Disconnected();
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo, "[" + Thread.CurrentThread.Name + "] Connection::Disconnect()");
            }
        }

        /// <summary>
        /// A friendly name for this connection.
        /// </summary>
        /// <returns>The Name property</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Adds a parser class to a list of custom parsers. 
        /// Any number can be added. The custom parsers
        /// will be tested using <c>CanParse()</c> before
        /// the default parsers. The last parser to be added
        /// will be the first to process a message.
        /// </summary>
        /// <param name="parser">Any class that implements IParser.</param>
        public void AddParser(IParser parser)
        {
            parsers.Insert(0, parser);
        }

        /// <summary>
        /// Remove a custom parser class.
        /// </summary>
        /// <param name="parser">Any class that implements IParser.</param>
        public void RemoveParser(IParser parser)
        {
            parsers.Remove(parser);
        }


        protected override void OnConnect()
        {
            ConnectionEstablished.Fire(this, new EventArgs());
            sender.RegisterConnection(connectionArgs);
            Connected = true;
        }

        protected override bool OnCertificateValidatecateFailed(X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            throw new NotImplementedException();
        }

        protected override void OnDisconnect(Exception reason)
        {
            if (ConnectionLost != null)
            {
                ConnectionLost(this, new FlamingDataEventArgs<string>(reason.Message)); //TODO: Better errors
            }
        }

        protected override void OnConnectFailed(int socketErrorCode)
        {
            if (ConnectFailed != null)
                ConnectFailed(this, new FlamingDataEventArgs<int>(socketErrorCode));
        }

        protected override void OnReceiveLine(string line)
        {
            ReceiveIRCMessages(line);
        }
    }
}

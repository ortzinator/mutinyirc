using System;

namespace FlamingIRC;

public interface IConnection
{
    ISender Sender { get; }
    Listener Listener { get; }
    ConnectionArgs ConnectionData { get; }
    bool Connected { get; }
    bool Registered { get; }
    bool HandleNickTaken { get; set; }
    ServerProperties ServerProperties { get; }
    void Connect();

    /// <summary>
    /// Connect, if no session and no connect attempt is live.
    /// </summary>
    /// <returns>True if this call started a connect attempt, false if one was live already.</returns>
    bool TryConnect();

    /// <summary>
    /// Send a QUIT if connected, and end the session or the connect attempt. Does nothing if
    /// the connection is idle.
    /// </summary>
    void Disconnect(string reason);
    event EventHandler ConnectionEstablished;
    event EventHandler<ConnectFailedEventArgs> ConnectFailed;
    event EventHandler<DisconnectEventArgs> ConnectionLost;
    event EventHandler<DataEventArgs<string>> RawMessageReceived;
}

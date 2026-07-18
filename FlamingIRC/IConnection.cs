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
    void Disconnect(string reason);
    event EventHandler ConnectionEstablished;
    event EventHandler<ConnectFailedEventArgs> ConnectFailed;
    event EventHandler<DisconnectEventArgs> ConnectionLost;
    event EventHandler<DataEventArgs<string>> RawMessageReceived;
}

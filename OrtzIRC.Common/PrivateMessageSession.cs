namespace OrtzIRC.Common
{
    using System;
    using FlamingIRC;

    public sealed class PrivateMessageSession : MessageContext
    {
        public PrivateMessageSession(Server parentServer, User user)
        {
            Server = parentServer;
            User = user;
        }

        public Server Server { get; private set; }
        public User User { get; private set; }
        public event EventHandler<DataEventArgs<string>> MessageReceived;
        public event EventHandler<DataEventArgs<string>> MessageSent;

        public void Send(string message)
        {
            Server.MessageUser(User.Nick, message);
            MessageSent.Fire(this, new DataEventArgs<string>(message));
        }

        public void OnMessageReceived(DataEventArgs<string> e)
        {
            MessageReceived.Fire(this, e);
        }
    }
}

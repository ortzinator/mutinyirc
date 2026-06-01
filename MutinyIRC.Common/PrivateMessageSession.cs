using System;
using FlamingIRC;

namespace MutinyIRC.Common
{
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
        public event EventHandler<DataEventArgs<string>> ActionReceived;
        public event EventHandler<DataEventArgs<string>> ActionSent;

        public void Send(string message)
        {
            Server.MessageUser(User.Nick, message);
            MessageSent.Fire(this, new DataEventArgs<string>(message));
        }

        public void SendAction(string message)
        {
            Server.MessageUserAction(User.Nick, message);
            ActionSent.Fire(this, new DataEventArgs<string>(message));
        }

        public void OnMessageReceived(DataEventArgs<string> e)
        {
            MessageReceived.Fire(this, e);
        }

        public void OnActionReceived(DataEventArgs<string> e)
        {
            ActionReceived.Fire(this, e);
        }
    }
}

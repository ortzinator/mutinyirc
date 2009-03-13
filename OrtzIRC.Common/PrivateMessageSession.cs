namespace OrtzIRC.Common
{
    using System;

    public class PrivateMessageSession : MessageContext
    {
        public PrivateMessageSession(Server parentServer, FlamingIRC.User user)
        {
            ParentServer = parentServer;
            User = user;
        }

        public Server ParentServer { get; private set; }
        public FlamingIRC.User User { get; private set; }
        public event EventHandler<DataEventArgs<string>> MessageReceived;

        public void Send(string message)
        {
            // todo - send message
        }

        protected virtual void OnMessageReceived(DataEventArgs<string> e)
        {
            MessageReceived.Fire(this, e);
        }
    }
}

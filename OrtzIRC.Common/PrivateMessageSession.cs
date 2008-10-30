namespace OrtzIRC.Common
{
    using System;

    public class PrivateMessageSession
    {
        public event EventHandler<DataEventArgs<string>> MessageReceived;

        public Server ParentServer { get; private set; }
        public FlamingIRC.User User { get; private set; }

        public PrivateMessageSession(Server parentServer, FlamingIRC.User user)
        {
            this.ParentServer = parentServer;
        }

        public void Send(string message)
        {
            // todo - send message
        }

        protected virtual void OnMessageReceived(DataEventArgs<string> e)
        {
            this.MessageReceived.Fire<DataEventArgs<string>>(this, e);
        }
    }
}

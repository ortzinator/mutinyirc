namespace OrtzIRC.Commands
{
    using System;
    using OrtzIRC.Common;

    [Command]
    public class Part
    {
        public void Execute(Channel channel)
        {
            channel.Part("Goodbye!"); 
        }

        public void Execute(Channel channel, string message)
        {
            channel.Part(message);
        }
    }
}
﻿namespace OrtzIRC.Commands
{
    using System;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;

    [Plugin("Join", 
        "Ortzinator", 
        "1.0", 
        "Joins a channel")]
    public class Join : ICommand
    {
        [CommandAutocomplete("Joins a channel", "#channel")]
        public void Execute(Channel channel, params string[] parameters)
        {
            channel.Server.JoinChannel(parameters[0]);
        }

        [CommandAutocomplete("Joins a channel", "#channel")]
        public void Execute(Server server, params string[] parameters)
        {
            server.JoinChannel(parameters[0]);
        }

        [CommandAutocomplete("Joins a channel", "#channel")]
        public void Execute(PrivateMessageSession pmSession, params string[] parameters)
        {
            pmSession.ParentServer.JoinChannel(parameters[0]);
        }
    }
}

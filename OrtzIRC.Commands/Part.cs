namespace OrtzIRC.Commands
{
    using System;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;

    [Plugin("Part",
        "Ortzinator",
        "1.0",
        "Parts a channel")]
    public class Part : ICommand
    {
        [CommandAutocomplete("Parts the current channel", "message")]
        [CommandAutocomplete("Parts a channel", "#channel")]
        [CommandAutocomplete("Parts a channel", "#channel", "message")]
        public void Execute(Channel channel, params string[] parameters)
        {
            if (parameters.Length == 0)
            {
                channel.Part("Goodbye!");
            }
            else if (parameters.Length == 1)
            {
                if (channel.Server.ChanManager.InChannel(parameters[0]))
                {
                    channel.Server.ChanManager.GetChannel(parameters[0]).Part("Goodbye!");
                }
            }
            else if (parameters.Length == 2)
            {
                if (channel.Server.ChanManager.InChannel(parameters[0]))
                {
                    channel.Server.ChanManager.GetChannel(parameters[0]).Part(parameters[1]);
                }
            }
        }

        [CommandAutocomplete("Parts a channel", "#channel")]
        [CommandAutocomplete("Parts a channel", "#channel", "message")]
        public void Execute(Server server, params string[] parameters)
        {
            if (parameters.Length == 1)
            {
                if (server.ChanManager.InChannel(parameters[0]))
                {
                    server.ChanManager.GetChannel(parameters[0]).Part("Goodbye!");
                }
            }
            else if (parameters.Length == 2)
            {
                if (server.ChanManager.InChannel(parameters[0]))
                {
                    server.ChanManager.GetChannel(parameters[0]).Part(parameters[1]);
                }
            }
        }

        [CommandAutocomplete("Parts a channel", "#channel")]
        [CommandAutocomplete("Parts a channel", "#channel", "message")]
        public void Execute(PrivateMessageSession pmSession, params string[] parameters)
        {
            if (parameters.Length == 1)
            {
                if (pmSession.ParentServer.ChanManager.InChannel(parameters[0]))
                {
                    pmSession.ParentServer.ChanManager.GetChannel(parameters[0]).Part("Goodbye!");
                }
            }
            else if (parameters.Length == 2)
            {
                if (pmSession.ParentServer.ChanManager.InChannel(parameters[0]))
                {
                    pmSession.ParentServer.ChanManager.GetChannel(parameters[0]).Part(parameters[1]);
                }
            }
        }
    }
}
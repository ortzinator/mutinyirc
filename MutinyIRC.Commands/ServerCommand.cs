using FlamingIRC;
using System;
using MutinyIRC.Common;
using System.Configuration;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.Commands;

/// <summary>
/// Creates new connections to IRC servers
/// </summary>
[Plugin("Server", "Connects to an IRC server. Usage: /server [-n] <server> [port]")]
public class ServerCommand : ICommand
{
    /// <summary>
    /// Summary of command with these specific parameters
    /// </summary>
    public void Execute(Server context, char[] switches, string server)
    {
        Execute(context, switches, server, "6667");
    }

    /// <summary>
    /// Handles switch-prefixed connects (e.g. <c>-n</c> for a new window) on the given port.
    /// </summary>
    public void Execute(Server context, char[] switches, string server, string port)
    {
        foreach (char c in switches)
        {
            switch (c)
            {
                case 'n': //New window and connect
                    var args = new ConnectionArgs(ConfigurationManager.AppSettings["FirstNick"] ?? "MutinyIRC", server, false);
                    try
                    {
                        args.Port = int.Parse(port);
                    }
                    catch (Exception)
                    {
                        args.Port = 6667;
                    }
                    var svr = ServerManager.Instance.Create(args);
                    svr.Connect();
                    return;
            }
        }
    }

    /// <summary>
    /// Connect to a server in the same window
    /// </summary>
    public void Execute(Server context, string server)
    {
        context.ChangeServer(ConfigurationManager.AppSettings["FirstNick"] ?? "MutinyIRC", server, false);
        context.Connect();
    }

    /// <summary>
    /// Connect to a server on the given port in the same window.
    /// </summary>
    public void Execute(Server context, string server, string port)
    {
        var args = new ConnectionArgs(ConfigurationManager.AppSettings["FirstNick"] ?? "MutinyIRC", server, false);
        try
        {
            args.Port = int.Parse(port);
        }
        catch (Exception)
        {
            args.Port = 6667;
        }

        context.ChangeServer(args);
        context.Connect();
    }
}

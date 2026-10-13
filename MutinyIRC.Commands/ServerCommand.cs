using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.Commands;

/// <summary>
/// Creates new connections to IRC servers
/// </summary>
/// <remarks>
/// With no port, the command dials <see cref="IrcPorts.DefaultTlsPort"/>. The port decides TLS
/// through <see cref="IrcPorts.IsTlsPort"/>, the same rule as the Add Server box in the settings.
/// </remarks>
[Plugin("Server", "Connects to an IRC server. Usage: /server [-n] <server> [port]")]
public class ServerCommand : ICommand
{
    private const string Usage = "Usage: /server [-n] <server> [port]";

    /// <summary>
    /// Handles switch-prefixed connects (e.g. <c>-n</c> for a new window) on the default port.
    /// </summary>
    public CommandResultInfo Execute(Server context, char[] switches, string server)
    {
        return Connect(context, switches, server, null);
    }

    /// <summary>
    /// Handles switch-prefixed connects (e.g. <c>-n</c> for a new window) on the given port.
    /// </summary>
    public CommandResultInfo Execute(Server context, char[] switches, string server, string port)
    {
        return Connect(context, switches, server, port);
    }

    /// <summary>
    /// Connect to a server in the same window
    /// </summary>
    public CommandResultInfo Execute(Server context, string server)
    {
        return Connect(context, new char[0], server, null);
    }

    /// <summary>
    /// Connect to a server on the given port in the same window.
    /// </summary>
    public CommandResultInfo Execute(Server context, string server, string port)
    {
        return Connect(context, new char[0], server, port);
    }

    private static CommandResultInfo Connect(Server context, char[] switches, string server, string port)
    {
        bool newWindow = false;
        foreach (char c in switches)
        {
            if (c == 'n')
                newWindow = true;
            else
                return CommandResultInfo.Fail(Usage);
        }

        int portNumber = IrcPorts.DefaultTlsPort;
        if (port != null && !IrcPorts.TryParsePort(port, out portNumber))
            return CommandResultInfo.Fail($"{port} is not a valid port. {Usage}");

        var args = new ConnectionArgs(context.UserNick, server, IrcPorts.IsTlsPort(portNumber))
        {
            Port = portNumber,
        };

        if (newWindow)
        {
            ServerManager.Instance.Create(args).Connect();
        }
        else
        {
            context.ChangeServer(args);
            context.Connect();
        }

        return CommandResultInfo.Success(null);
    }
}

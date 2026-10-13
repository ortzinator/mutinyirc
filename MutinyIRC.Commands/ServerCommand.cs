using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.Commands;

/// <summary>
/// Creates new connections to IRC servers
/// </summary>
/// <remarks>
/// The command reads the server and port through <see cref="EntryPoint.TryParse"/>, so it
/// accepts <c>host:port</c> as well as a separate port. With no port, it dials the TLS port. From a channel or query window, the command acts on the server that owns the window.
/// </remarks>
[Plugin("Server", "Connects to an IRC server. Usage: /server [-n] <server> [port]")]
public class ServerCommand : ICommand
{
    private const string Usage = "Usage: /server [-n] <server> [port]";

    /// <summary>
    /// Handles switch-prefixed connects (e.g. <c>-n</c> for a new window) on the default port.
    /// </summary>
    public CommandResultInfo Execute(MessageContext context, char[] switches, string server)
    {
        return Connect(context.OwningServer, switches, server, null);
    }

    /// <summary>
    /// Handles switch-prefixed connects (e.g. <c>-n</c> for a new window) on the given port.
    /// </summary>
    public CommandResultInfo Execute(MessageContext context, char[] switches, string server, string port)
    {
        return Connect(context.OwningServer, switches, server, port);
    }

    /// <summary>
    /// Connects the owning server to the given host on the default port.
    /// </summary>
    public CommandResultInfo Execute(MessageContext context, string server)
    {
        return Connect(context.OwningServer, new char[0], server, null);
    }

    /// <summary>
    /// Connects the owning server to the given host on the given port.
    /// </summary>
    public CommandResultInfo Execute(MessageContext context, string server, string port)
    {
        return Connect(context.OwningServer, new char[0], server, port);
    }

    private static CommandResultInfo Connect(Server owner, char[] switches, string server, string port)
    {
        bool newWindow = false;
        foreach (char c in switches)
        {
            if (c == 'n')
                newWindow = true;
            else
                return CommandResultInfo.Fail(Usage);
        }

        string text = port == null ? server : $"{server}:{port}";
        if (!EntryPoint.TryParse(text, out EntryPoint entry))
            return CommandResultInfo.Fail($"{text} is not a valid server. {Usage}");

        var args = new ConnectionArgs(owner.UserNick, entry.Host, entry.Ssl)
        {
            Port = entry.Port,
        };

        if (newWindow)
        {
            ServerManager.Instance.Create(args).Connect();
        }
        else
        {
            owner.ChangeServer(args);
            owner.Connect();
        }

        return CommandResultInfo.Success(null);
    }
}

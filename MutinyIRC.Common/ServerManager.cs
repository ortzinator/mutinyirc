using System.Linq;
using System.Collections.Generic;
using System;
using FlamingIRC;

namespace MutinyIRC.Common;

public class ServerManager
{
    public event EventHandler<ServerEventArgs> ServerAdded;
    public event EventHandler<ServerEventArgs> ServerRemoved;

    private static ServerManager Singleton;

    public List<Server> ServerList { get; private set; }

    /// <summary>
    /// The trust lookup that every new <see cref="Server"/> gets as its
    /// <see cref="Server.CertificateTrustLookup"/>. The host sets it once, at startup, so a
    /// connection that a command creates gets it too.
    /// </summary>
    public Func<string, int, CertificateTrust> CertificateTrustLookup { get; set; }

    public static ServerManager Instance
    {
        get
        {
            Singleton ??= new ServerManager { ServerList = new List<Server>() };

            return Singleton;
        }
    }

    public Server Create(ConnectionArgs args)
    {
        return Add(new Server(args));
    }

    /// <summary>
    /// Creates a connection that dials <paramref name="candidates"/> in order until one answers.
    /// </summary>
    public Server Create(IReadOnlyList<ConnectionArgs> candidates)
    {
        return Add(new Server(candidates));
    }

    private Server Add(Server newServer)
    {
        newServer.CertificateTrustLookup = CertificateTrustLookup;
        ServerList.Add(newServer);

        ServerAdded.Fire(this, new ServerEventArgs(newServer));

        return newServer;
    }

    public void Remove(Server ntw)
    {
        if (ServerList.Remove(ntw))
            ServerRemoved.Fire(this, new ServerEventArgs(ntw));
    }

    public bool AnyConnected()
    {
        return ServerList.Any(server => server.IsConnected);
    }

    public void DisconnectAll()
    {
        foreach (var server in ServerList)
        {
            if (server.IsConnected)
                server.Disconnect();
        }
    }
}

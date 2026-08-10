using System;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers what a connection writes back to settings when it registers, and which channels it
/// autojoins as a result. The interesting case is a host that no saved network lists, reached
/// on a network that is saved: the connection must find that network by the name the server
/// reports, add itself to it, and join its channels.
/// </summary>
[TestFixture]
public class ServerViewModelRegistrationTests
{
    private static readonly FieldInfo SingletonField =
        typeof(IrcSettingsManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static)!;

    private Server _server = null!;
    private ServerViewModel _vm = null!;

    [TearDown]
    public void Teardown()
    {
        _vm?.Dispose();
        // Clear the singleton so the next consumer loads normally instead of seeing this fixture's data.
        SingletonField.SetValue(null, null);
    }

    /// <summary>
    /// Swaps a fresh <see cref="IrcSettingsManager"/> into the singleton field so no test reads or
    /// writes the user's real servers.json. The constructor is private and the field is static, so
    /// reflection is the only way in without adding a production seam that only tests would use.
    /// </summary>
    private static IrcSettingsManager UseEmptySettings()
    {
        var manager = (IrcSettingsManager)Activator.CreateInstance(typeof(IrcSettingsManager), nonPublic: true)!;
        SingletonField.SetValue(null, manager);
        return manager;
    }

    /// <summary>
    /// Builds a view model over a faked connection to <paramref name="host"/>. The connection never
    /// opens a socket, but its real <see cref="Listener"/> parses raw lines, so registration runs
    /// exactly as it does against a live server.
    /// </summary>
    private void Dial(string host)
    {
        var args = new ConnectionArgs("me", host, false);
        var conn = A.Fake<Connection>(x => x.WithArgumentsForConstructor(new object[] { args, false, false }));

        _server = new Server(conn);
        _vm = new ServerViewModel(_server, new PluginManager());
    }

    /// <summary>
    /// Drives a real registration: the 005 that announces the network name, then the 001 that
    /// completes registration and fires <see cref="Server.Registered"/>. Pass an empty name to
    /// register against a server that announces no network.
    /// </summary>
    private void Register(string reportedNetworkName)
    {
        if (reportedNetworkName != string.Empty)
            _server.Connection.Listener.Parse(
                $":irc.fake.com 005 me NETWORK={reportedNetworkName} CHANTYPES=# :are supported by this server");

        _server.Connection.Listener.Parse(":irc.fake.com 001 me :Welcome to the Internet Relay Network");
    }

    private static NetworkSettings SaveLibera(IrcSettingsManager manager)
    {
        NetworkSettings libera = manager.GetOrAddNetwork("Libera");
        libera.AddServer(new ServerSettings("irc.libera.chat", "Random", "6667", false));
        libera.AddChannel(new ChannelSettings("#MutinyIRC", true));
        return libera;
    }

    [Test]
    public void Registering_NewHostOfKnownNetwork_JoinsThatNetworksAutoJoinChannels()
    {
        // eu.libera.chat is not saved, but the server reports "Libera", which is saved and
        // carries an autojoin list. The connection belongs to that network, so its channels
        // must be joined on this connect — not only on the next one.
        IrcSettingsManager manager = UseEmptySettings();
        SaveLibera(manager);

        Dial("eu.libera.chat");
        Register("Libera");

        Assert.That(_server.Channels.Keys, Does.Contain("#MutinyIRC"),
            "A host that reports a saved network's name must autojoin that network's channels on the first connect");
    }

    [Test]
    public void Registering_NewHostOfKnownNetwork_AddsTheHostToThatNetwork()
    {
        IrcSettingsManager manager = UseEmptySettings();
        NetworkSettings libera = SaveLibera(manager);

        Dial("eu.libera.chat");
        Register("Libera");

        Assert.That(manager.Networks.Count, Is.EqualTo(1),
            "A host reporting a saved network's name must join that network, not mint a second one");
        Assert.That(libera.Servers.Select(s => s.Url),
            Is.EqualTo(new[] { "irc.libera.chat", "eu.libera.chat" }));
    }

    [Test]
    public void Registering_SavedHost_JoinsItsNetworksAutoJoinChannels()
    {
        IrcSettingsManager manager = UseEmptySettings();
        SaveLibera(manager);

        Dial("irc.libera.chat");
        Register("Libera");

        Assert.That(_server.Channels.Keys, Does.Contain("#MutinyIRC"));
    }

    [Test]
    public void Registering_UnknownHostAndNoReportedName_MintsNetworkNamedForTheHost()
    {
        IrcSettingsManager manager = UseEmptySettings();

        Dial("irc.example.net");
        Register(string.Empty);

        Assert.That(manager.Networks.Select(n => n.Name), Is.EqualTo(new[] { "irc.example.net" }));
        Assert.That(manager.Networks[0].Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc.example.net" }));
    }

    [Test]
    public void Registering_ReportedNameChanged_RenamesTheNetworkItBelongsTo()
    {
        // The server owns the name and may change it on any connect. The network the host
        // already belongs to takes the new name rather than a second network appearing.
        IrcSettingsManager manager = UseEmptySettings();
        NetworkSettings libera = SaveLibera(manager);

        Dial("irc.libera.chat");
        Register("LiberaChat");

        Assert.That(manager.Networks.Count, Is.EqualTo(1));
        Assert.That(libera.Name, Is.EqualTo("LiberaChat"));
    }
}

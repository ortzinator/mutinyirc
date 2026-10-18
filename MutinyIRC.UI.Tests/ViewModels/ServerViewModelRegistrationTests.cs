using System;
using System.Collections.Generic;
using System.IO;
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
        typeof(IrcSettingsManager).GetField("Singleton", BindingFlags.NonPublic | BindingFlags.Static)!;

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
    /// writes the user's real servers.json. The manager is pointed at a file of this fixture's own,
    /// so even a Save cannot reach it; the singleton field is static, so reflection is still the
    /// only way to put the manager where <c>ServerViewModel</c> looks for it.
    /// </summary>
    private static IrcSettingsManager UseEmptySettings()
    {
        var manager = new IrcSettingsManager(
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json"));
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
        libera.AddServer(new ServerSettings("irc.libera.chat", 6667, false));
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

    /// <summary>
    /// Hands nick collisions to the view model, as <c>Server.CreateConnection</c> does in the
    /// app, and collects the NICK commands the connection tries to send.
    /// </summary>
    private List<string> CaptureNickCommands()
    {
        _server.Connection.HandleNickTaken = false;
        var sent = new List<string>();
        ((Connection)_server.Connection).RawMessageSent += (_, e) =>
        {
            if (e.Data.StartsWith("NICK ", StringComparison.Ordinal))
                sent.Add(e.Data);
        };
        return sent;
    }

    private const string NickInUse = ":irc.fake.com 433 me taken :Nickname is already in use";

    [Test]
    public void NickInUse_BeforeRegistration_RetriesWithAnotherNick()
    {
        UseEmptySettings();
        Dial("irc.example.net");
        List<string> sent = CaptureNickCommands();

        _server.Connection.Listener.Parse(NickInUse);

        Assert.That(sent, Has.Count.EqualTo(1),
            "A nick collision during registration must make the client try another nick");
    }

    [Test]
    public void NickInUse_AfterRegistration_KeepsTheCurrentNick()
    {
        // After registration a 433 answers the user's own /nick. The user still holds the old
        // nick, so the client must not pick a new one for them.
        UseEmptySettings();
        Dial("irc.example.net");
        Register(string.Empty);
        List<string> sent = CaptureNickCommands();

        _server.Connection.Listener.Parse(NickInUse);

        Assert.That(sent, Is.Empty,
            "A nick collision after registration must not rename the user");
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

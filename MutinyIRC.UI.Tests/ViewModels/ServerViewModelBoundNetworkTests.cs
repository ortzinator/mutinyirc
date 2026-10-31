using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Avalonia.Headless.NUnit;
using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers the bound network of a connection: the network that the connection was dialed for.
/// The connection keeps it across a dialog save, a host edit and a move of its entry point, and
/// it does not find its network by host again.
/// </summary>
/// <remarks>
/// The view model writes settings on the UI thread only, so every test runs on it.
/// </remarks>
[TestFixture]
public class ServerViewModelBoundNetworkTests
{
    private static readonly FieldInfo SingletonField =
        typeof(IrcSettingsManager).GetField("Singleton", BindingFlags.NonPublic | BindingFlags.Static)!;

    private const string Fingerprint = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    private IrcSettingsManager _settings = null!;
    private Server _server = null!;
    private ServerViewModel _vm = null!;

    [SetUp]
    public void Setup()
    {
        // A file of this fixture's own: the test must never reach the user's servers.json.
        _settings = new IrcSettingsManager(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json"));
        SingletonField.SetValue(null, _settings);
    }

    [TearDown]
    public void Teardown()
    {
        _vm?.Dispose();
        SingletonField.SetValue(null, null);
    }

    private void Dial(string host, int port = 6667, bool ssl = false)
    {
        var args = new ConnectionArgs("me", host, ssl) { Port = port };
        _server = new Server(A.Fake<Connection>(x => x.WithArgumentsForConstructor(new object[] { args, false, false })));
        _vm = new ServerViewModel(_server, new PluginManager());
    }

    private void Register(string reportedNetworkName = "")
    {
        _server.Connection.Listener.Parse(":irc.fake.com 001 me :Welcome to the Internet Relay Network");
        if (reportedNetworkName != string.Empty)
            _server.Connection.Listener.Parse(
                $":irc.fake.com 005 me NETWORK={reportedNetworkName} CHANTYPES=# :are supported by this server");
    }

    private NetworkSettings SaveNetwork(string name, string host, string channel, bool autoConnect = true)
    {
        NetworkSettings network = _settings.GetOrAddNetwork(name);
        network.AutoConnect = autoConnect;
        network.AddServer(new ServerSettings(host, 6667, false));
        network.AddChannel(new ChannelSettings(channel, true));
        return network;
    }

    private static void Move(ServerSettings entryPoint, NetworkSettings from, NetworkSettings to)
    {
        from.RemoveServer(entryPoint);
        to.AddServer(entryPoint);
    }

    [AvaloniaTest]
    public void ABoundConnection_KeepsItsNetworkObjectAcrossADialogSave()
    {
        NetworkSettings libera = SaveNetwork("Libera", "irc.libera.chat", "#MutinyIRC");
        Dial("irc.libera.chat");
        _vm.BindNetwork(libera);

        var dialog = new SettingsViewModel(_settings);
        dialog.Networks[0].Name = "LiberaRenamed";
        dialog.Save();
        Register();

        Assert.Multiple(() =>
        {
            Assert.That(_vm.BoundNetwork, Is.SameAs(libera));
            Assert.That(_settings.Networks.Single(), Is.SameAs(libera));
            Assert.That(_vm.NetworkName, Is.EqualTo("LiberaRenamed"));
        });
    }

    [AvaloniaTest]
    public void AfterAHostEdit_RegistrationDoesNotPutTheOldHostBack()
    {
        NetworkSettings libera = SaveNetwork("Libera", "irc.libera.chat", "#MutinyIRC");
        Dial("irc.libera.chat");
        _vm.BindNetwork(libera);

        libera.Servers[0].Url = "irc.eu.libera.chat";
        Register("Libera");

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks.Single(), Is.SameAs(libera),
                "The old host must not mint a network of its own");
            Assert.That(libera.Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc.eu.libera.chat" }));
            Assert.That(_vm.BoundNetwork, Is.SameAs(libera));
            Assert.That(_server.Channels.Keys, Does.Contain("#MutinyIRC"));
        });
    }

    [AvaloniaTest]
    public void AfterAMove_RegistrationDoesNotTakeOverTheTargetNetwork()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home");
        NetworkSettings other = SaveNetwork("Other", "irc.other.net", "#other", autoConnect: false);
        Dial("bouncer.home");
        _vm.BindNetwork(home);

        Move(home.Servers[0], home, other);
        Register("ReportedName");

        Assert.Multiple(() =>
        {
            Assert.That(other.Name, Is.EqualTo("Other"), "The target network must not be renamed");
            Assert.That(other.AutoConnect, Is.False, "The target network's AutoConnect must not be set");
            Assert.That(_server.Channels.Keys, Does.Not.Contain("#other"),
                "The target network's channels must not be joined");
            Assert.That(_server.Channels.Keys, Does.Contain("#home"),
                "The connection stays with its bound network and joins its channels");
            Assert.That(_vm.BoundNetwork, Is.SameAs(home));
            Assert.That(home.Servers, Is.Empty, "Registration must not put the moved entry point back");
        });
    }

    [AvaloniaTest]
    public void AfterAMove_APartRemovesTheChannelFromTheBoundNetworkOnly()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#shared");
        NetworkSettings other = SaveNetwork("Other", "irc.other.net", "#shared");
        Dial("bouncer.home");
        _vm.BindNetwork(home);
        Register();

        Move(home.Servers[0], home, other);
        _server.Connection.Listener.Parse(":me!u@h PART #shared");

        Assert.Multiple(() =>
        {
            Assert.That(home.Channels, Is.Empty);
            Assert.That(other.Channels.Select(c => c.Name), Is.EqualTo(new[] { "#shared" }));
        });
    }

    [AvaloniaTest]
    public void AnUnboundConnection_BindsAtItsFirstRegistration_AndDoesNotResolveByHostAgain()
    {
        // No saved network lists the host, so registration mints one and binds to it.
        Dial("irc.example.net");
        Register();
        NetworkSettings minted = _settings.Networks.Single();
        Assert.That(_vm.BoundNetwork, Is.SameAs(minted), "Precondition: bound at registration");

        // The user edits the host. The next registration of the same connection keeps the
        // binding and does not add the old host back.
        minted.Servers[0].Url = "irc2.example.net";
        Register();

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks.Single(), Is.SameAs(minted));
            Assert.That(minted.Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc2.example.net" }));
        });
    }

    [AvaloniaTest]
    public void AnUnboundConnection_StillSettlesIntoTheNetworkWithTheReportedName()
    {
        NetworkSettings libera = SaveNetwork("Libera", "irc.libera.chat", "#MutinyIRC");
        Dial("eu.libera.chat");

        Register("Libera");

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks.Single(), Is.SameAs(libera));
            Assert.That(_vm.BoundNetwork, Is.SameAs(libera));
            Assert.That(libera.Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc.libera.chat", "eu.libera.chat" }));
        });
    }

    [AvaloniaTest]
    public void ChangeServer_UnbindsTheConnection_SoTheNewHostResolvesByHost()
    {
        // /server without -n points the connection at a host the user names. The connection
        // has no network for that host yet.
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home");
        SaveNetwork("Other", "irc.other.net", "#other");
        Dial("bouncer.home");
        _vm.BindNetwork(home);

        _server.ChangeServer(new ConnectionArgs("me", "irc.other.net", false));

        Assert.That(_vm.BoundNetwork, Is.Null);
    }

    [AvaloniaTest]
    public void TheTrustLookup_TakesTheSwitchFromTheBoundNetworkAndThePinFromItsEntryPoint()
    {
        var home = new NetworkSettings("Home") { VerifyCertificates = false };
        home.AddServer(new ServerSettings("bouncer.home", 6697, true) { PinnedFingerprint = Fingerprint });
        _settings.Networks.ReplaceAll(new[] { home });
        Dial("bouncer.home", 6697, true);
        _vm.BindNetwork(home);

        CertificateTrust trust = _vm.GetCertificateTrust("bouncer.home", 6697);

        Assert.Multiple(() =>
        {
            Assert.That(trust.VerifyCertificates, Is.False);
            Assert.That(trust.PinnedFingerprint, Is.EqualTo(Fingerprint));
        });
    }

    [AvaloniaTest]
    public void TheTrustLookup_AfterAMove_UsesTheBoundNetworksSwitchAndNoPin()
    {
        var home = new NetworkSettings("Home") { VerifyCertificates = false };
        var other = new NetworkSettings("Other") { VerifyCertificates = true };
        var entryPoint = new ServerSettings("bouncer.home", 6697, true) { PinnedFingerprint = Fingerprint };
        home.AddServer(entryPoint);
        _settings.Networks.ReplaceAll(new[] { home, other });
        Dial("bouncer.home", 6697, true);
        _vm.BindNetwork(home);

        Move(entryPoint, home, other);
        CertificateTrust trust = _vm.GetCertificateTrust("bouncer.home", 6697);

        Assert.Multiple(() =>
        {
            Assert.That(trust.VerifyCertificates, Is.False, "The switch comes from the bound network");
            Assert.That(trust.PinnedFingerprint, Is.Null, "The bound network does not list the entry point");
        });
    }

    [AvaloniaTest]
    public void TheTrustLookup_OfAnUnboundConnection_FindsTheNetworkByHost()
    {
        var home = new NetworkSettings("Home") { VerifyCertificates = false };
        home.AddServer(new ServerSettings("bouncer.home", 6697, true) { PinnedFingerprint = Fingerprint });
        _settings.Networks.ReplaceAll(new[] { home });
        Dial("bouncer.home", 6697, true);

        CertificateTrust trust = _vm.GetCertificateTrust("bouncer.home", 6697);

        Assert.Multiple(() =>
        {
            Assert.That(trust.VerifyCertificates, Is.False);
            Assert.That(trust.PinnedFingerprint, Is.EqualTo(Fingerprint));
        });
    }

    [AvaloniaTest]
    public void TheAlternateNick_ComesFromTheBoundNetwork_OnASocketThread()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home");
        home.AlternateNick = "homeAlt";
        Dial("bouncer.home");
        _vm.BindNetwork(home);

        // A move must not change the alternate nick: it is the bound network's.
        NetworkSettings other = SaveNetwork("Other", "irc.other.net", "#other");
        other.AlternateNick = "otherAlt";
        Move(home.Servers[0], home, other);

        _server.Connection.HandleNickTaken = false;
        string? sent = null;
        ((Connection)_server.Connection).RawMessageSent += (_, e) =>
        {
            if (e.Data.StartsWith("NICK ", StringComparison.Ordinal))
                sent = e.Data;
        };

        var thread = new Thread(() =>
            _server.Connection.Listener.Parse(":irc.fake.com 433 me taken :Nickname is already in use"));
        thread.Start();
        thread.Join();

        Assert.That(sent, Is.EqualTo("NICK homeAlt"));
    }
}

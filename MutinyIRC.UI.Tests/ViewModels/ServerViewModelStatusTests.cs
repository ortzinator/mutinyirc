using System.Linq;
using System.Reflection;
using Avalonia.Headless.NUnit;
using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.Resources;
using MutinyIRC.UI.ViewModels;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers the two lines the network switcher reads off a connection: which network it is, and
/// where it stands. Both are strings the view model builds, so the tests pin the state each
/// event leaves behind rather than the wording.
/// </summary>
[TestFixture]
public class ServerViewModelStatusTests
{
    private static readonly FieldInfo SettingsSingleton =
        typeof(IrcSettingsManager).GetField("Singleton", BindingFlags.NonPublic | BindingFlags.Static)!;

    private Server _server = null!;
    private ServerViewModel _vm = null!;

    [SetUp]
    public void Setup()
    {
        // Registration files the connection under a network, so give it settings of its own,
        // over a file of this fixture's own, instead of the user's real servers.json.
        SettingsSingleton.SetValue(null, new IrcSettingsManager(
            System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                                   System.IO.Path.GetRandomFileName() + ".json")));
    }

    [TearDown]
    public void Teardown()
    {
        _vm?.Dispose();
        _server?.Dispose();
        SettingsSingleton.SetValue(null, null);
    }

    /// <summary>
    /// A view model over a faked connection. Its <see cref="Listener"/> is real, so raw lines
    /// drive registration exactly as a live server would.
    /// </summary>
    private void Dial(string host = "irc.fake.com", bool ssl = false)
    {
        var args = new ConnectionArgs("me", host, ssl);
        var conn = A.Fake<Connection>(x => x.WithArgumentsForConstructor(new object[] { args, false, false }));
        _server = new Server(conn);
        _vm = new ServerViewModel(_server, new PluginManager());
    }

    private void Register(string reportedNetworkName)
    {
        if (reportedNetworkName != string.Empty)
            _server.Connection.Listener.Parse(
                $":irc.fake.com 005 me NETWORK={reportedNetworkName} CHANTYPES=# :are supported by this server");

        _server.Connection.Listener.Parse(":irc.fake.com 001 me :Welcome");
    }

    [Test]
    public void BeforeConnecting_IsNamedForTheHostAndReportsDisconnected()
    {
        Dial("irc.libera.chat");

        Assert.That(_vm.NetworkName, Is.EqualTo("irc.libera.chat"),
            "Until it registers, the host we dialled is the only name we have for the network");
        Assert.That(_vm.ConnectionStatus, Is.EqualTo("Disconnected"));
    }

    [Test]
    public void OnceRegistered_IsNamedForTheNetworkTheServerReports()
    {
        Dial("eu.libera.chat");

        Register("Libera");

        Assert.That(_vm.NetworkName, Is.EqualTo("Libera"));
    }

    [Test]
    public void OnceRegistered_ReportsConnected()
    {
        Dial();

        Register("Libera");

        Assert.That(_vm.ConnectionStatus, Is.EqualTo("Connected"));
    }

    [Test]
    public void OnceRegisteredOverTls_SaysSo()
    {
        Dial(ssl: true);

        Register("Libera");

        Assert.That(_vm.ConnectionStatus, Is.EqualTo("Connected · TLS"),
            "TLS is worth calling out; the rest of the connection state reads the same");
    }

    [Test]
    public void WhileDialling_ReportsConnecting()
    {
        Dial();

        _server.Connect();

        Assert.That(_vm.ConnectionStatus, Is.EqualTo("Connecting…"));
    }

    /// <summary>
    /// A view model over a real (never-opened) connection, for the tests that need a drop: the
    /// disconnect path runs inside <see cref="Connection"/>, so a faked one never reaches it.
    /// </summary>
    private Connection DialOffline()
    {
        var conn = new Connection(new ConnectionArgs("me", "irc.fake.com", false), false, false);
        _server = new Server(conn);
        _vm = new ServerViewModel(_server, new PluginManager());
        return conn;
    }

    [Test]
    public void AfterADrop_ReportsTheReconnectAttempt()
    {
        Connection conn = DialOffline();

        conn.Disconnect(DisconnectReason.SocketError);
        Assert.That(_vm.ConnectionStatus, Is.EqualTo("Reconnecting · retry 1"));

        conn.Disconnect(DisconnectReason.SocketError);
        Assert.That(_vm.ConnectionStatus, Is.EqualTo("Reconnecting · retry 2"),
            "Each further try counts up, so a connection that keeps failing says how many times");
    }

    [Test]
    public void AfterAUserInitiatedQuit_ReportsDisconnected()
    {
        Connection conn = DialOffline();
        Register("Libera");
        Assert.That(_vm.ConnectionStatus, Is.EqualTo("Connected"), "Precondition: registered");

        conn.Disconnect(DisconnectReason.UserInitiated);

        Assert.That(_vm.ConnectionStatus, Is.EqualTo("Disconnected"),
            "A quit the user asked for ends the connection; nothing is going to reconnect it");
    }

    /// <summary>
    /// Raises ConnectFailed with <paramref name="failure"/> over a TLS connection. The
    /// connection is a faked <see cref="IConnection"/>, because only a fake lets the test raise
    /// the event. A real <see cref="Connection"/> raises it only from the transport.
    /// </summary>
    private void DialAndFail(ConnectFailedEventArgs failure)
    {
        var conn = A.Fake<IConnection>();
        // A real Listener: Server attaches to its events, and a fake's default is null.
        A.CallTo(() => conn.Listener).Returns(new Listener());
        A.CallTo(() => conn.ConnectionData).Returns(new ConnectionArgs("me", "irc.fake.com", true));
        _server = new Server(conn);
        _vm = new ServerViewModel(_server, new PluginManager());

        conn.ConnectFailed += Raise.With(conn, failure);
    }

    [AvaloniaTest]
    public void CertificateRejected_TellsTheUserTheCertificateIsNotTrusted()
    {
        DialAndFail(new ConnectFailedEventArgs(ConnectError.CertificateRejected));

        Assert.That(_vm.ChatLines.Select(l => l.Message), Does.Contain(ServerStrings.CertificateRejectedMessage),
            "A rejected certificate must say so, not report socket error 0");
    }

    [AvaloniaTest]
    public void EveryEntryPointRefused_SaysMutinyStoppedTrying()
    {
        // One entry point, so its refusal is a refusal from every entry point.
        DialAndFail(new ConnectFailedEventArgs(ConnectError.CertificateRejected));

        Assert.Multiple(() =>
        {
            Assert.That(_vm.ConnectionStatus, Is.EqualTo("Certificate refused"));
            Assert.That(_vm.ChatLines.OfType<ErrorMessageViewModel>().Select(l => l.Message),
                Does.Contain("Every server refused its certificate. Mutiny stopped trying. " +
                             "Trust a certificate, or reconnect."));
        });
    }

    [AvaloniaTest]
    public void HandshakeFailure_IsNotReportedAsACertificateRejection()
    {
        DialAndFail(new ConnectFailedEventArgs(ConnectError.AuthenticationFailed));

        Assert.That(_vm.ChatLines.Select(l => l.Message), Does.Contain(ServerStrings.TlsHandshakeFailedMessage));
        Assert.That(_vm.ChatLines.Select(l => l.Message), Does.Not.Contain(ServerStrings.CertificateRejectedMessage),
            "A handshake can fail while the certificate is good, so it must not blame the certificate");
    }

    [AvaloniaTest]
    [TestCase(ConnectError.CertificateRejected)]
    [TestCase(ConnectError.AuthenticationFailed)]
    [TestCase(ConnectError.ConnectionRefused)]
    [TestCase(ConnectError.Other)]
    public void FailureWithNoSocketCode_DoesNotReportSocketErrorZero(ConnectError reason)
    {
        DialAndFail(new ConnectFailedEventArgs(reason));

        Assert.That(_vm.ChatLines.Select(l => l.Message), Has.None.Contains("Socket error code"),
            "Only a SocketError has a socket error code to show");
    }

    [AvaloniaTest]
    public void SocketFailure_StillReportsTheSocketError()
    {
        DialAndFail(new ConnectFailedEventArgs(ConnectError.SocketError, 10061));

        Assert.That(_vm.ChatLines.Select(l => l.Message), Does.Not.Contain(ServerStrings.CertificateRejectedMessage),
            "Only a certificate rejection may use the certificate wording");
        Assert.That(_vm.ChatLines.Select(l => l.Message),
            Does.Contain(ServerStrings.ConnectionFailedMessage.With(SocketErrorTranslator.GetMessage(10061))));
    }

    /// <summary>
    /// Raises ConnectionLost with <paramref name="loss" /> from a faked <see cref="IConnection" />.
    /// </summary>
    private void DialAndLose(DisconnectEventArgs loss)
    {
        var conn = A.Fake<IConnection>();
        // A real Listener: Server attaches to its events, and a fake's default is null.
        A.CallTo(() => conn.Listener).Returns(new Listener());
        A.CallTo(() => conn.ConnectionData).Returns(new ConnectionArgs("me", "irc.fake.com", false));
        _server = new Server(conn);
        _vm = new ServerViewModel(_server, new PluginManager());

        conn.ConnectionLost += Raise.With(conn, loss);
    }

    [AvaloniaTest]
    [TestCase(DisconnectReason.RemoteHostClosedConnection)]
    [TestCase(DisconnectReason.PingTimeout)]
    [TestCase(DisconnectReason.Other)]
    public void LossWithNoSocketCode_DoesNotReportSocketErrorZero(DisconnectReason reason)
    {
        DialAndLose(new DisconnectEventArgs(reason));

        Assert.That(_vm.ChatLines.Select(l => l.Message), Has.None.Contains("Socket error code"),
            "Only a SocketError has a socket error code to show");
    }

    [AvaloniaTest]
    public void ServerClose_SaysTheServerClosedTheConnection()
    {
        DialAndLose(new DisconnectEventArgs(DisconnectReason.RemoteHostClosedConnection));

        Assert.That(_vm.ChatLines.Select(l => l.Message), Does.Contain(ServerStrings.ConnectionLostServerClosed));
    }

    [AvaloniaTest]
    public void PingTimeout_SaysTheServerStoppedResponding()
    {
        DialAndLose(new DisconnectEventArgs(DisconnectReason.PingTimeout));

        Assert.That(_vm.ChatLines.Select(l => l.Message), Does.Contain(ServerStrings.ConnectionLostPingTimeout));
    }

    [AvaloniaTest]
    public void SocketLoss_StillReportsTheSocketError()
    {
        DialAndLose(new DisconnectEventArgs(DisconnectReason.SocketError, 10054));

        Assert.That(_vm.ChatLines.Select(l => l.Message),
            Does.Contain(ServerStrings.ConnectionLost.With(SocketErrorTranslator.GetMessage(10054))));
    }
}

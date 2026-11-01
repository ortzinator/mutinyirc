using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
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
/// Covers a live connection whose bound network lost the entry points it needs: its last entry
/// point went, or the dialog deleted the network. The session stays up, and a drop stops the
/// connection with a message. Registration does not write a deleted network back. A refusal of
/// an entry point that the bound network does not list now cannot be trusted.
/// </summary>
/// <remarks>The view model writes settings on the UI thread only, so every test runs on it.</remarks>
[TestFixture]
public class ServerViewModelLostEntryPointsTests
{
    private static readonly FieldInfo SettingsSingleton =
        typeof(IrcSettingsManager).GetField("Singleton", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly PropertyInfo ListenerProperties =
        typeof(Listener).GetProperty("ServerProperties", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private const string First = "a.fake.com";
    private const string Second = "b.fake.com";
    private const int Port = 6697;

    /// <summary>
    /// A connection whose dials only record the entry point, so a test can refuse a
    /// certificate, register or drop at will.
    /// </summary>
    private sealed class WalkingServer : Server
    {
        public List<string> Dialled { get; } = new();

        // The base constructor sets it through CreateConnection. IDE0370 is a false positive
        // here: without the '!' the build gives CS8618.
#pragma warning disable IDE0370
        public IConnection Current { get; private set; } = null!;
#pragma warning restore IDE0370

        public WalkingServer(IReadOnlyList<ConnectionArgs> candidates) : base(candidates) { }

        protected override IConnection CreateConnection(ConnectionArgs args)
        {
            var conn = A.Fake<IConnection>();
            var listener = new Listener();
            A.CallTo(() => conn.Listener).Returns(listener);
            // Registration reads the network name from the server properties. Only FlamingIRC can
            // make them, so take the ones that the real Listener holds.
            A.CallTo(() => conn.ServerProperties).Returns((ServerProperties)ListenerProperties.GetValue(listener)!);
            A.CallTo(() => conn.ConnectionData).Returns(args);
            A.CallTo(() => conn.TryConnect())
                .Invokes(() => Dialled.Add($"{args.Hostname}:{args.Port}"))
                .Returns(true);
            Current = conn;
            return conn;
        }

        public void RefuseCurrent(X509Certificate2 certificate) =>
            Current.ConnectFailed += Raise.With(Current,
                new ConnectFailedEventArgs(ConnectError.CertificateRejected,
                    new RejectedCertificate(certificate.RawData, SslPolicyErrors.RemoteCertificateChainErrors)));

        public void RegisterCurrent() => Current.Listener.Parse($":{Current.ConnectionData.Hostname} 001 me :Welcome");

        public void ReportNetworkName(string name) =>
            Current.Listener.Parse($":{Current.ConnectionData.Hostname} 005 me NETWORK={name} :are supported by this server");

        public void DropCurrent() =>
            Current.ConnectionLost += Raise.With(Current,
                new DisconnectEventArgs(DisconnectReason.RemoteHostClosedConnection));
    }

    private X509Certificate2 _certificate = null!;
    private string _settingsPath = null!;
    private IrcSettingsManager _settings = null!;
    private NetworkSettings _network = null!;
    private WalkingServer _server = null!;
    private ServerViewModel _vm = null!;

    [OneTimeSetUp]
    public void CreateCertificate()
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=fake.com", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        _certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
    }

    [OneTimeTearDown]
    public void DisposeCertificate() => _certificate.Dispose();

    [SetUp]
    public void Setup()
    {
        // A file of this fixture's own: the test must never reach the user's servers.json.
        _settingsPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");
        _settings = new IrcSettingsManager(_settingsPath);
        SettingsSingleton.SetValue(null, _settings);

        // The nick is set, so the dial list does not read the app settings.
        _network = new NetworkSettings("Home") { Nick = "me" };
        _network.AddServer(new ServerSettings(First, Port, true));
        _network.AddServer(new ServerSettings(Second, Port, true));
        _settings.Networks.ReplaceAll(new[] { _network });
    }

    [TearDown]
    public void Teardown()
    {
        _vm?.Dispose();
        _server?.Dispose();
        SettingsSingleton.SetValue(null, null);
        File.Delete(_settingsPath);
    }

    private void Dial(NetworkSettings network)
    {
        _server = new WalkingServer(network.ToConnectionCandidates())
        {
            // As MainViewModel sets it, less the marshal to the UI thread: the tests run there.
            CertificateTrustLookup = (host, port) => _vm!.GetCertificateTrust(host, port)
        };
        _vm = new ServerViewModel(_server, new PluginManager());
        _vm.BindNetwork(network);
        _server.Connect();
    }

    private void DialAndRegister()
    {
        Dial(_network);
        _server.RegisterCurrent();
    }

    /// <summary>Deletes <paramref name="network"/> in the Networks dialog and clicks OK.</summary>
    private void DeleteInDialog(NetworkSettings network)
    {
        var dialog = new SettingsViewModel(_settings);
        dialog.SelectedNetwork = dialog.Networks.Single(row => row.Source == network);
        dialog.RemoveNetworkCommand.Execute(null);
        dialog.Save();
        _vm.FollowSavedEntryPoints();
    }

    /// <summary>Removes every entry point of the bound network, as a save would.</summary>
    private void RemoveEveryEntryPoint()
    {
        foreach (ServerSettings entryPoint in _network.Servers.ToList())
            _network.RemoveServer(entryPoint);
        _vm.FollowSavedEntryPoints();
    }

    private int StopCount() =>
        _vm.ChatLines.Count(line => line.Message == ServerStrings.NoEntryPointsMessage);

    private int ReconnectNoticeCount() =>
        _vm.ChatLines.Count(line => line is not ErrorMessageViewModel
                                    && line.Message.StartsWith("Attempting", StringComparison.Ordinal));

    private CertificateRefusalViewModel Refusal() =>
        _vm.ChatLines.OfType<CertificateRefusalViewModel>().Single();

    // ── No entry points left ─────────────────────────────────────────────────

    [AvaloniaTest]
    public void NoEntryPoints_TheSessionStaysUp()
    {
        DialAndRegister();
        IConnection live = _server.Current;

        RemoveEveryEntryPoint();

        Assert.Multiple(() =>
        {
            A.CallTo(() => live.Disconnect(A<string>._)).MustNotHaveHappened();
            Assert.That(StopCount(), Is.Zero, "nothing stops before the session drops");
        });
    }

    [AvaloniaTest]
    public void NoEntryPoints_ADrop_StopsWithTheMessageAndNoReconnect()
    {
        DialAndRegister();
        RemoveEveryEntryPoint();
        int dials = _server.Dialled.Count;

        _server.DropCurrent();

        Assert.Multiple(() =>
        {
            Assert.That(StopCount(), Is.EqualTo(1));
            Assert.That(_vm.ConnectionStatus, Is.EqualTo(ServerStrings.StatusDisconnected));
            Assert.That(_server.Dialled.Count, Is.EqualTo(dials), "no dial after the stop");
            Assert.That(ReconnectNoticeCount(), Is.Zero, "no reconnect is scheduled");
        });
    }

    [AvaloniaTest]
    public void NoEntryPoints_Message_HasTheAgreedText()
    {
        Assert.That(ServerStrings.NoEntryPointsMessage,
            Is.EqualTo("This network has no entry points. Add one in the Networks dialog."));
    }

    // ── The network was deleted ──────────────────────────────────────────────

    [AvaloniaTest]
    public void NetworkDeleted_TheSessionStaysUp()
    {
        DialAndRegister();
        IConnection live = _server.Current;

        DeleteInDialog(_network);

        A.CallTo(() => live.Disconnect(A<string>._)).MustNotHaveHappened();
    }

    [AvaloniaTest]
    public void NetworkDeleted_ADrop_StopsWithTheMessageAndNoReconnect()
    {
        DialAndRegister();
        DeleteInDialog(_network);
        int dials = _server.Dialled.Count;

        _server.DropCurrent();

        Assert.Multiple(() =>
        {
            Assert.That(StopCount(), Is.EqualTo(1));
            Assert.That(_server.Dialled.Count, Is.EqualTo(dials));
        });
    }

    [AvaloniaTest]
    public void NetworkDeleted_ANetworkNameReport_DoesNotWriteTheNetworkBack()
    {
        DialAndRegister();
        DeleteInDialog(_network);

        _server.ReportNetworkName("HomeNet");

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks, Is.Empty, "the network stays deleted");
            Assert.That(_network.Name, Is.EqualTo("Home"));
        });
    }

    [AvaloniaTest]
    public void NetworkDeleted_AMintedNetwork_ANetworkNameReport_DoesNotMergeItsEntryPoint()
    {
        // A saved network that has the name the server will report.
        var libera = new NetworkSettings("Libera") { Nick = "me" };
        libera.AddServer(new ServerSettings("irc.libera.chat", Port, true));
        _settings.Networks.ReplaceAll(new[] { libera });
        NetworkSettings minted = _settings.MintPinnedNetwork(First, Port, "AB")!;
        minted.Nick = "me";
        Dial(minted);
        _server.RegisterCurrent();
        DeleteInDialog(minted);

        _server.ReportNetworkName("Libera");

        Assert.Multiple(() =>
        {
            Assert.That(libera.Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc.libera.chat" }));
            Assert.That(_settings.Networks, Is.EqualTo(new[] { libera }));
            Assert.That(libera.AutoConnect, Is.False);
        });
    }

    [AvaloniaTest]
    public void NetworkDeleted_ARegistration_DoesNotWriteTheNetworkBack()
    {
        // The dial is still in flight when the dialog deletes the network.
        Dial(_network);
        DeleteInDialog(_network);

        _server.RegisterCurrent();
        _server.ReportNetworkName("HomeNet");

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks, Is.Empty);
            Assert.That(_network.AutoConnect, Is.False);
            Assert.That(_network.Name, Is.EqualTo("Home"));
        });
    }

    // ── Trust on a removed entry point ───────────────────────────────────────

    [AvaloniaTest]
    public void TrustOnARemovedEntryPoint_IsDisabledWithTheReason()
    {
        Dial(_network);
        _server.RefuseCurrent(_certificate);   // a.fake.com refuses; the walk moves to b.fake.com

        _network.RemoveServer(_network.GetServer(First)!);
        _vm.FollowSavedEntryPoints();

        CertificateRefusalViewModel refusal = Refusal();
        Assert.Multiple(() =>
        {
            Assert.That(refusal.TrustCommand.CanExecute(null), Is.False);
            Assert.That(refusal.CanTrust, Is.True, "the button stays in view, disabled");
            Assert.That(refusal.TrustLabel, Is.EqualTo(ServerStrings.TrustEntryPointRemovedButton));
        });
    }

    [AvaloniaTest]
    public async Task TrustOnAMovedEntryPoint_PinsNothingAndMintsNothing()
    {
        Dial(_network);
        _server.RefuseCurrent(_certificate);
        CertificateRefusalViewModel refusal = Refusal();

        // The move: a.fake.com goes to another network.
        ServerSettings moved = _network.GetServer(First)!;
        _network.RemoveServer(moved);
        var other = new NetworkSettings("Other");
        other.AddServer(moved);
        _settings.Networks.ReplaceAll(new[] { _network, other });
        _vm.FollowSavedEntryPoints();
        int dials = _server.Dialled.Count;

        await refusal.TrustCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(moved.PinnedFingerprint, Is.Null);
            Assert.That(_settings.Networks.Count, Is.EqualTo(2), "no network is minted");
            Assert.That(_server.Dialled.Count, Is.EqualTo(dials));
        });
    }

    [AvaloniaTest]
    public async Task TrustAfterTheNetworkWasDeleted_IsDisabledAndMintsNothing()
    {
        Dial(_network);
        _server.RefuseCurrent(_certificate);
        CertificateRefusalViewModel refusal = Refusal();

        DeleteInDialog(_network);
        await refusal.TrustCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(refusal.TrustCommand.CanExecute(null), Is.False);
            Assert.That(_settings.Networks, Is.Empty, "no network is minted");
        });
    }

    [AvaloniaTest]
    public void ARefusalOfAnEntryPointTheNetworkNoLongerLists_IsDisabledFromTheStart()
    {
        // The dial to a.fake.com is in flight when a save removes it.
        Dial(_network);
        _network.RemoveServer(_network.GetServer(First)!);
        _vm.FollowSavedEntryPoints();

        _server.RefuseCurrent(_certificate);

        Assert.That(Refusal().TrustCommand.CanExecute(null), Is.False);
    }

    [AvaloniaTest]
    public void TrustOnAnEntryPointTheNetworkStillLists_StaysEnabled()
    {
        Dial(_network);
        _server.RefuseCurrent(_certificate);

        _network.RemoveServer(_network.GetServer(Second)!);
        _vm.FollowSavedEntryPoints();

        CertificateRefusalViewModel refusal = Refusal();
        Assert.Multiple(() =>
        {
            Assert.That(refusal.TrustCommand.CanExecute(null), Is.True);
            Assert.That(refusal.TrustLabel, Is.EqualTo(ServerStrings.TrustCertificateButton));
        });
    }
}

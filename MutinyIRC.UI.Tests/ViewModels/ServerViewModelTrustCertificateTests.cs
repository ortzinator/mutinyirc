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
using MutinyIRC.UI.ViewModels;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers the "Trust this certificate" button on a refusal in the server window: it pins the
/// refused certificate on the refused entry point and dials that entry point at once. Only the
/// newest refusal has a working button, and replacing a pin needs a confirm.
/// </summary>
[TestFixture]
public class ServerViewModelTrustCertificateTests
{
    private static readonly FieldInfo SettingsSingleton =
        typeof(IrcSettingsManager).GetField("Singleton", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly PropertyInfo ListenerProperties =
        typeof(Listener).GetProperty("ServerProperties", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private const string First = "a.fake.com";
    private const string Second = "b.fake.com";
    private const int Port = 6697;

    /// <summary>
    /// A connection over two TLS entry points whose dials only record the entry point, so a
    /// test can refuse a certificate or register at will.
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
    }

    private X509Certificate2 _certificate = null!;
    private X509Certificate2 _otherCertificate = null!;

    private IrcSettingsManager _settings = null!;
    private NetworkSettings _network = null!;
    private WalkingServer _server = null!;
    private ServerViewModel _vm = null!;

    [OneTimeSetUp]
    public void CreateCertificates()
    {
        _certificate = SelfSigned();
        _otherCertificate = SelfSigned();
    }

    [OneTimeTearDown]
    public void DisposeCertificates()
    {
        _certificate.Dispose();
        _otherCertificate.Dispose();
    }

    private static X509Certificate2 SelfSigned()
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=fake.com", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
    }

    /// <summary>The fingerprint, worked out apart from the code under test.</summary>
    private static string FingerprintOf(X509Certificate2 certificate) =>
        Convert.ToHexString(SHA256.HashData(certificate.RawData));

    [SetUp]
    public void Setup()
    {
        _settings = new IrcSettingsManager(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json"));
        SettingsSingleton.SetValue(null, _settings);

        _network = new NetworkSettings("Home");
        _network.AddServer(new ServerSettings(First, Port, true));
        _network.AddServer(new ServerSettings(Second, Port, true));
        _settings.Networks.ReplaceAll(new[] { _network });

        _server = new WalkingServer(_network.ToConnectionCandidates())
        {
            // As MainViewModel sets it, less the marshal to the UI thread: the tests run there.
            CertificateTrustLookup = (host, port) => _settings.GetCertificateTrust(host, port)
        };
        _vm = new ServerViewModel(_server, new PluginManager());
        _server.Connect();
    }

    [TearDown]
    public void Teardown()
    {
        _vm?.Dispose();
        _server?.Dispose();
        SettingsSingleton.SetValue(null, null);
    }

    private IReadOnlyList<CertificateRefusalViewModel> Refusals() =>
        _vm.ChatLines.OfType<CertificateRefusalViewModel>().ToList();

    private string? PinOf(string host) => _settings.GetCertificateTrust(host, Port).PinnedFingerprint;

    [AvaloniaTest]
    public void ARefusal_OffersToTrustTheCertificate()
    {
        _server.RefuseCurrent(_certificate);

        CertificateRefusalViewModel refusal = Refusals().Single();
        Assert.Multiple(() =>
        {
            Assert.That(refusal.CanTrust, Is.True);
            Assert.That(refusal.TrustLabel, Is.EqualTo("Trust this certificate"));
            Assert.That(refusal.TrustCommand.CanExecute(null), Is.True);
        });
    }

    [AvaloniaTest]
    public async Task Trust_PinsTheRefusedEntryPointAndDialsItAtOnce()
    {
        _server.RefuseCurrent(_certificate);   // a.fake.com refuses; the walk moves to b.fake.com
        Assert.That(_server.Dialled.Last(), Is.EqualTo($"{Second}:{Port}"), "Precondition: the walk moved on");

        await Refusals().Single().TrustCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(PinOf(First), Is.EqualTo(FingerprintOf(_certificate)));
            Assert.That(PinOf(Second), Is.Null, "the pin belongs only to the refused entry point");
            Assert.That(_server.Dialled.Last(), Is.EqualTo($"{First}:{Port}"),
                "the refused entry point is dialled again at once");
            Assert.That(Refusals().Single().CanTrust, Is.False, "the button goes once it is used");
        });
    }

    [AvaloniaTest]
    public async Task Trust_ForAnEntryPointThatNoSavedNetworkLists_SaysSoAndDialsNothing()
    {
        _server.RefuseCurrent(_certificate);
        _settings.Networks.ReplaceAll(Array.Empty<NetworkSettings>());
        int dials = _server.Dialled.Count;

        await Refusals().Single().TrustCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(_server.Dialled, Has.Count.EqualTo(dials), "nothing is dialled");
            Assert.That(_vm.ChatLines.OfType<ErrorMessageViewModel>().Last().Message, Is.EqualTo(
                "Mutiny can pin a certificate only for a server that a saved network lists. "
                + "Add a.fake.com:6697 to a network in the Networks dialog, then connect again."));
        });
    }

    [AvaloniaTest]
    public void OnlyTheNewestRefusal_HasAWorkingButton()
    {
        _server.RefuseCurrent(_certificate);
        _server.RefuseCurrent(_otherCertificate);

        IReadOnlyList<CertificateRefusalViewModel> refusals = Refusals();
        Assert.Multiple(() =>
        {
            Assert.That(refusals.Select(r => r.CanTrust), Is.EqualTo(new[] { false, true }));
            Assert.That(refusals[0].TrustCommand.CanExecute(null), Is.False);
        });
    }

    [AvaloniaTest]
    public void ASuccessfulConnect_RemovesTheButton()
    {
        _server.RefuseCurrent(_certificate);

        _server.RegisterCurrent();

        Assert.That(Refusals().Single().CanTrust, Is.False);
    }

    [AvaloniaTest]
    public void AChangedPin_OffersToReplaceThePin()
    {
        _network.Servers[0].PinnedFingerprint = FingerprintOf(_otherCertificate);

        _server.RefuseCurrent(_certificate);

        Assert.That(Refusals().Single().TrustLabel, Is.EqualTo("Replace pinned certificate"));
    }

    [AvaloniaTest]
    public async Task ReplaceThenConfirm_ReplacesThePinAndDials()
    {
        _network.Servers[0].PinnedFingerprint = FingerprintOf(_otherCertificate);
        CertificateRefusalViewModel? asked = null;
        _vm.ConfirmPinReplacement = refusal =>
        {
            asked = refusal;
            return Task.FromResult(true);
        };
        _server.RefuseCurrent(_certificate);

        await Refusals().Single().TrustCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(asked, Is.SameAs(Refusals().Single()), "the confirm is asked first");
            Assert.That(PinOf(First), Is.EqualTo(FingerprintOf(_certificate)));
            Assert.That(_server.Dialled.Last(), Is.EqualTo($"{First}:{Port}"));
        });
    }

    [AvaloniaTest]
    public async Task ReplaceThenCancel_ChangesNothing()
    {
        _network.Servers[0].PinnedFingerprint = FingerprintOf(_otherCertificate);
        _vm.ConfirmPinReplacement = _ => Task.FromResult(false);
        _server.RefuseCurrent(_certificate);
        int dials = _server.Dialled.Count;

        await Refusals().Single().TrustCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(PinOf(First), Is.EqualTo(FingerprintOf(_otherCertificate)));
            Assert.That(_server.Dialled, Has.Count.EqualTo(dials), "nothing is dialled");
            Assert.That(Refusals().Single().CanTrust, Is.True, "the user may still decide later");
        });
    }
}

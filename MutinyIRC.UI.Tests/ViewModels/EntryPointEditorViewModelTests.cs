using System.IO;
using System.Linq;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers the editor that an entry point row becomes: the draft fields, the port that a blank
/// field gets, the pin warning, the live clash refusal, and what Apply and Cancel write.
/// </summary>
[TestFixture]
public class EntryPointEditorViewModelTests
{
    private const string Fingerprint =
        "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    private string _settingsFile = null!;

    [SetUp]
    public void SetUp() =>
        _settingsFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

    [TearDown]
    public void TearDown() => File.Delete(_settingsFile);

    private static NetworkSettingsViewModel Network(params ServerSettings[] servers)
    {
        var model = new NetworkSettings("TestNet");
        foreach (ServerSettings server in servers)
            model.AddServer(server);
        return new NetworkSettingsViewModel(model);
    }

    private static ServerSettings Pinned(string host, int port) =>
        new(host, port, true) { PinnedFingerprint = Fingerprint };

    private static EntryPointEditorViewModel Edit(NetworkSettingsViewModel network, int row)
    {
        network.EditServerCommand.Execute(network.Servers[row]);
        return network.Servers[row].Editor!;
    }

    // ── Open, Apply, Cancel ──────────────────────────────────────────────────

    [Test]
    public void Edit_OpensTheEditorWithTheRowValues()
    {
        NetworkSettingsViewModel network = Network(new ServerSettings("irc.example.net", 7000, true));

        EntryPointEditorViewModel editor = Edit(network, 0);

        Assert.Multiple(() =>
        {
            Assert.That(network.Servers[0].IsEditing, Is.True);
            Assert.That(editor.Host, Is.EqualTo("irc.example.net"));
            Assert.That(editor.Port, Is.EqualTo("7000"));
            Assert.That(editor.Ssl, Is.True);
            Assert.That(editor.ApplyCommand.CanExecute(null), Is.True,
                "an editor with no change can apply");
        });
    }

    [Test]
    public void Apply_WritesTheDraftToTheRowAndClosesTheEditor()
    {
        NetworkSettingsViewModel network = Network(
            new ServerSettings("irc.example.net", 6667, false),
            new ServerSettings("eu.example.net", 6667, false));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Host = "us.example.net";
        editor.Port = "7000";
        editor.Ssl = true;
        editor.ApplyCommand.Execute(null);

        ServerSettingsViewModel row = network.Servers[0];
        Assert.Multiple(() =>
        {
            Assert.That(row.Url, Is.EqualTo("us.example.net"));
            Assert.That(row.Port, Is.EqualTo(7000));
            Assert.That(row.Ssl, Is.True);
            Assert.That(row.IsEditing, Is.False);
            Assert.That(row.Editor, Is.Null);
            Assert.That(network.Servers.Select(s => s.Url),
                Is.EqualTo(new[] { "us.example.net", "eu.example.net" }),
                "an edit keeps the place of the entry point in the failover order");
        });
    }

    [Test]
    public void Apply_TrimsTheHostAndThePort()
    {
        NetworkSettingsViewModel network = Network(new ServerSettings("irc.example.net", 6667, false));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Host = "  us.example.net ";
        editor.Port = " 7000 ";
        editor.ApplyCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(network.Servers[0].Url, Is.EqualTo("us.example.net"));
            Assert.That(network.Servers[0].Port, Is.EqualTo(7000));
        });
    }

    [Test]
    public void Cancel_KeepsTheRowAndClosesTheEditor()
    {
        NetworkSettingsViewModel network = Network(Pinned("irc.example.net", 6697));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Host = "us.example.net";
        editor.Port = "7000";
        editor.Ssl = false;
        editor.CancelCommand.Execute(null);

        ServerSettingsViewModel row = network.Servers[0];
        Assert.Multiple(() =>
        {
            Assert.That(row.Url, Is.EqualTo("irc.example.net"));
            Assert.That(row.Port, Is.EqualTo(6697));
            Assert.That(row.Ssl, Is.True);
            Assert.That(row.PinnedFingerprint, Is.EqualTo(Fingerprint), "a cancel keeps the pin");
            Assert.That(row.IsEditing, Is.False);
        });
    }

    [Test]
    public void Edit_AgainAfterCancel_StartsFromTheRowValues()
    {
        NetworkSettingsViewModel network = Network(new ServerSettings("irc.example.net", 6667, false));
        EntryPointEditorViewModel first = Edit(network, 0);
        first.Host = "us.example.net";
        first.CancelCommand.Execute(null);

        EntryPointEditorViewModel second = Edit(network, 0);

        Assert.That(second.Host, Is.EqualTo("irc.example.net"));
    }

    [Test]
    public void Edit_OfARowThatIsOpen_KeepsTheDraft()
    {
        NetworkSettingsViewModel network = Network(new ServerSettings("irc.example.net", 6667, false));
        EntryPointEditorViewModel editor = Edit(network, 0);
        editor.Host = "us.example.net";

        network.EditServerCommand.Execute(network.Servers[0]);

        Assert.That(network.Servers[0].Editor, Is.SameAs(editor));
    }

    // ── TLS and the port ─────────────────────────────────────────────────────

    [TestCase("6667", false)]
    [TestCase("6697", true)]
    [TestCase("7000", true)]
    [TestCase("7000", false)]
    public void TogglingTls_NeverChangesThePort(string port, bool sslBefore)
    {
        NetworkSettingsViewModel network = Network(new ServerSettings("irc.example.net", 6667, false));
        EntryPointEditorViewModel editor = Edit(network, 0);
        editor.Port = port;
        editor.Ssl = sslBefore;

        editor.Ssl = !sslBefore;
        Assert.That(editor.Port, Is.EqualTo(port));

        editor.ApplyCommand.Execute(null);
        Assert.Multiple(() =>
        {
            Assert.That(network.Servers[0].Port, Is.EqualTo(int.Parse(port)));
            Assert.That(network.Servers[0].Ssl, Is.EqualTo(!sslBefore));
        });
    }

    [TestCase(true, 6697)]
    [TestCase(false, 6667)]
    public void ABlankPort_SavesAsTheDefaultForTheTlsFlag(bool ssl, int expected)
    {
        NetworkSettingsViewModel network = Network(new ServerSettings("irc.example.net", 7000, !ssl));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Port = "  ";
        editor.Ssl = ssl;
        Assert.That(editor.PortPlaceholder, Is.EqualTo(expected.ToString()));

        editor.ApplyCommand.Execute(null);
        Assert.That(network.Servers[0].Port, Is.EqualTo(expected));
    }

    [TestCase("0")]
    [TestCase("65536")]
    [TestCase("port")]
    [TestCase("-1")]
    public void AnInvalidPort_DisablesApply(string port)
    {
        NetworkSettingsViewModel network = Network(new ServerSettings("irc.example.net", 6667, false));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Port = port;

        Assert.That(editor.ApplyCommand.CanExecute(null), Is.False);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("irc.example.net:6697")]
    [TestCase("irc example net")]
    public void AnInvalidHost_DisablesApply(string host)
    {
        NetworkSettingsViewModel network = Network(new ServerSettings("irc.example.net", 6667, false));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Host = host;

        Assert.That(editor.ApplyCommand.CanExecute(null), Is.False);
    }

    // ── The pin ──────────────────────────────────────────────────────────────

    [Test]
    public void PinWarning_IsOffForAnUnchangedOrCaseOnlyEdit()
    {
        NetworkSettingsViewModel network = Network(Pinned("irc.example.net", 6697));
        EntryPointEditorViewModel editor = Edit(network, 0);

        Assert.That(editor.PinWillClear, Is.False, "an unchanged draft keeps the pin");

        editor.Host = "IRC.Example.NET";
        Assert.That(editor.PinWillClear, Is.False, "a hostname is not case-sensitive");

        editor.Port = string.Empty;
        Assert.That(editor.PinWillClear, Is.False, "a blank port with TLS on is 6697, the same port");
    }

    [Test]
    public void PinWarning_ShowsForANewHost()
    {
        NetworkSettingsViewModel network = Network(Pinned("irc.example.net", 6697));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Host = "us.example.net";

        Assert.That(editor.PinWillClear, Is.True);
    }

    [Test]
    public void PinWarning_ShowsForANewPort()
    {
        NetworkSettingsViewModel network = Network(Pinned("irc.example.net", 6697));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Port = "7000";

        Assert.That(editor.PinWillClear, Is.True);
    }

    [Test]
    public void PinWarning_ShowsForTlsOff()
    {
        NetworkSettingsViewModel network = Network(Pinned("irc.example.net", 7000));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Ssl = false;

        Assert.That(editor.PinWillClear, Is.True);
    }

    [Test]
    public void PinWarning_IsOffForARowWithNoPin()
    {
        NetworkSettingsViewModel network = Network(new ServerSettings("irc.example.net", 6697, true));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Host = "us.example.net";
        editor.Ssl = false;

        Assert.That(editor.PinWillClear, Is.False);
    }

    [Test]
    public void PinWarning_GoesWhenThePinIsForgottenDuringTheEdit()
    {
        NetworkSettingsViewModel network = Network(Pinned("irc.example.net", 6697));
        EntryPointEditorViewModel editor = Edit(network, 0);
        editor.Host = "us.example.net";
        bool raised = false;
        editor.PropertyChanged += (_, e) =>
            raised |= e.PropertyName == nameof(EntryPointEditorViewModel.PinWillClear);

        network.Servers[0].ForgetPinCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(editor.PinWillClear, Is.False);
            Assert.That(raised, Is.True, "the view must hear that the warning went");
        });
    }

    [TestCase("us.example.net", "6697", true)]
    [TestCase("irc.example.net", "7000", true)]
    [TestCase("irc.example.net", "6697", false)]
    public void Apply_ClearsThePinForANewHostOrPortOrTlsOff(string host, string port, bool ssl)
    {
        NetworkSettingsViewModel network = Network(Pinned("irc.example.net", 6697));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Host = host;
        editor.Port = port;
        editor.Ssl = ssl;
        editor.ApplyCommand.Execute(null);

        Assert.That(network.Servers[0].PinnedFingerprint, Is.Null);
    }

    [TestCase("irc.example.net", "6697")]
    [TestCase("IRC.example.net", "")]
    public void Apply_KeepsThePinForOtherEdits(string host, string port)
    {
        NetworkSettingsViewModel network = Network(Pinned("irc.example.net", 6697));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Host = host;
        editor.Port = port;
        editor.ApplyCommand.Execute(null);

        Assert.That(network.Servers[0].PinnedFingerprint, Is.EqualTo(Fingerprint));
    }

    // ── The clash rule ───────────────────────────────────────────────────────

    [Test]
    public void AHostListedInThisNetwork_IsRefusedAndDisablesApply()
    {
        NetworkSettingsViewModel network = Network(
            new ServerSettings("irc.example.net", 6667, false),
            new ServerSettings("eu.example.net", 6697, true));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Host = "EU.example.net";

        Assert.Multiple(() =>
        {
            Assert.That(editor.Refusal, Is.EqualTo("EU.example.net is already listed in this network."));
            Assert.That(editor.HasRefusal, Is.True);
            Assert.That(editor.ApplyCommand.CanExecute(null), Is.False);
        });

        editor.Host = "us.example.net";

        Assert.Multiple(() =>
        {
            Assert.That(editor.HasRefusal, Is.False, "the refusal goes when the clash goes");
            Assert.That(editor.ApplyCommand.CanExecute(null), Is.True);
        });
    }

    [Test]
    public void AnEditOfOnlyThePortOrTls_NeverClashes()
    {
        NetworkSettingsViewModel network = Network(new ServerSettings("irc.example.net", 6667, false));
        EntryPointEditorViewModel editor = Edit(network, 0);

        editor.Host = "IRC.example.net";
        editor.Port = "6697";
        editor.Ssl = true;

        Assert.Multiple(() =>
        {
            Assert.That(editor.HasRefusal, Is.False, "a row is never a clash with itself");
            Assert.That(editor.ApplyCommand.CanExecute(null), Is.True);
        });
    }

    [Test]
    public void AHostListedInAnotherNetwork_IsRefusedWithItsName()
    {
        var vm = new SettingsViewModel(ManagerWith(
            NetworkModel("Libera", "irc.libera.chat"),
            NetworkModel("OFTC", "irc.oftc.net")));
        EntryPointEditorViewModel editor = Edit(vm.Networks[1], 0);

        editor.Host = "IRC.libera.chat";

        Assert.Multiple(() =>
        {
            Assert.That(editor.Refusal, Is.EqualTo("IRC.libera.chat is already listed in Libera."));
            Assert.That(editor.ApplyCommand.CanExecute(null), Is.False);
        });
    }

    [Test]
    public void TheRefusal_FollowsTheOtherRowsLive()
    {
        var vm = new SettingsViewModel(ManagerWith(
            NetworkModel("Libera", "irc.libera.chat"),
            NetworkModel("OFTC", "irc.oftc.net")));
        NetworkSettingsViewModel libera = vm.Networks[0];
        EntryPointEditorViewModel editor = Edit(vm.Networks[1], 0);
        editor.Host = "irc.libera.chat";
        Assert.That(editor.HasRefusal, Is.True);

        libera.RemoveServerCommand.Execute(libera.Servers[0]);
        Assert.That(editor.HasRefusal, Is.False, "an unsaved delete frees the host at once");

        libera.NewServerText = "irc.libera.chat";
        libera.AddServerCommand.Execute(null);
        Assert.That(editor.HasRefusal, Is.True, "an unsaved add counts at once");

        libera.Name = "Libera.Chat";
        Assert.That(editor.Refusal, Is.EqualTo("irc.libera.chat is already listed in Libera.Chat."),
            "the refusal names the network as it is now");
    }

    // ── The verify switch ────────────────────────────────────────────────────

    [Test]
    public void TlsOffOnTheLastTlsRow_DisablesTheVerifySwitchAndKeepsItsValue()
    {
        var model = new NetworkSettings("TestNet") { VerifyCertificates = false };
        model.AddServer(new ServerSettings("irc.example.net", 6697, true));
        model.AddServer(new ServerSettings("eu.example.net", 6667, false));
        var network = new NetworkSettingsViewModel(model);
        var raised = new System.Collections.Generic.List<string?>();
        network.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        EntryPointEditorViewModel editor = Edit(network, 0);
        editor.Ssl = false;
        editor.ApplyCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(network.CanVerifyCertificates, Is.False);
            Assert.That(network.VerifyCertificatesHint,
                Is.EqualTo("None of this network's servers use TLS, so there is no certificate to check."));
            Assert.That(network.IsVerifyCertificatesHintWarning, Is.False);
            Assert.That(raised, Does.Contain(nameof(NetworkSettingsViewModel.CanVerifyCertificates)));
            Assert.That(raised, Does.Contain(nameof(NetworkSettingsViewModel.VerifyCertificatesHint)));
            Assert.That(raised, Does.Contain(nameof(NetworkSettingsViewModel.IsVerifyCertificatesHintWarning)));
            Assert.That(network.VerifyCertificates, Is.False, "the saved value is kept");
        });

        editor = Edit(network, 1);
        editor.Ssl = true;
        editor.ApplyCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(network.CanVerifyCertificates, Is.True, "TLS is back");
            Assert.That(network.VerifyCertificates, Is.False, "the saved value comes back");
            Assert.That(network.IsVerifyCertificatesHintWarning, Is.True);
        });
    }

    private IrcSettingsManager ManagerWith(params NetworkSettings[] networks)
    {
        var manager = new IrcSettingsManager(_settingsFile);
        manager.Networks.ReplaceAll(networks);
        return manager;
    }

    private static NetworkSettings NetworkModel(string name, string host)
    {
        var net = new NetworkSettings(name);
        net.AddServer(new ServerSettings(host, 6667, false));
        return net;
    }
}

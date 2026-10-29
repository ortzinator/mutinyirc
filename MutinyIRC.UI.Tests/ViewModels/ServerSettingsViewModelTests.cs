using NUnit.Framework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers one entry point row of the Networks dialog: its pin line, Forget, and the edits that
/// clear the pin. The dialog edits a copy, so a cleared pin is gone only when the dialog saves.
/// </summary>
[TestFixture]
public class ServerSettingsViewModelTests
{
    private const string Fingerprint =
        "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    private static ServerSettingsViewModel Pinned() =>
        new(new ServerSettings("irc.example.net", 6697, true) { PinnedFingerprint = Fingerprint });

    [Test]
    public void APinnedEntryPoint_ShowsTheFirstBytesOfItsPin()
    {
        var vm = Pinned();

        Assert.Multiple(() =>
        {
            Assert.That(vm.IsPinned, Is.True);
            Assert.That(vm.PinLine, Is.EqualTo("Pinned 01:23:45:67:89:AB:CD:EF…"));
        });
    }

    [Test]
    public void AnUnpinnedEntryPoint_HasNoPinLine()
    {
        var vm = new ServerSettingsViewModel(new ServerSettings("irc.example.net", 6697, true));

        Assert.That(vm.IsPinned, Is.False);
    }

    [Test]
    public void Forget_ClearsThePin()
    {
        var vm = Pinned();

        vm.ForgetPinCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(vm.IsPinned, Is.False);
            Assert.That(vm.ToModel().PinnedFingerprint, Is.Null);
        });
    }

    [Test]
    public void ChangingTheHost_ClearsThePin()
    {
        var vm = Pinned();

        vm.Url = "irc.other.net";

        Assert.That(vm.ToModel().PinnedFingerprint, Is.Null);
    }

    [Test]
    public void ChangingOnlyTheCaseOfTheHost_KeepsThePin()
    {
        var vm = Pinned();

        vm.Url = "IRC.Example.NET";

        Assert.That(vm.ToModel().PinnedFingerprint, Is.EqualTo(Fingerprint),
            "a host name is not case-sensitive, so this is the same entry point");
    }

    [Test]
    public void ChangingThePort_ClearsThePin()
    {
        var vm = Pinned();

        vm.Port = 7000;

        Assert.That(vm.ToModel().PinnedFingerprint, Is.Null);
    }

    [Test]
    public void TurningTlsOff_ClearsThePin()
    {
        var vm = Pinned();

        vm.Ssl = false;

        Assert.That(vm.ToModel().PinnedFingerprint, Is.Null);
    }

    [Test]
    public void ChangingTheOrder_KeepsThePin()
    {
        var vm = Pinned();

        vm.Order = 2;
        vm.IsFirst = false;

        Assert.That(vm.ToModel().PinnedFingerprint, Is.EqualTo(Fingerprint));
    }
}

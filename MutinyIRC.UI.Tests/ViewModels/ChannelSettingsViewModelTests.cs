using NUnit.Framework;
using MutinyIRC.Common;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

[TestFixture]
public class ChannelSettingsViewModelTests
{
    [Test]
    public void Constructor_FromModel_CopiesName()
    {
        var model = new ChannelSettings("#mutiny", true, "desc", "secret");
        var vm = new ChannelSettingsViewModel(model);
        Assert.That(vm.Name, Is.EqualTo("#mutiny"));
    }

    [Test]
    public void Constructor_FromModel_CopiesKey()
    {
        var model = new ChannelSettings("#mutiny", true, "desc", "secret");
        var vm = new ChannelSettingsViewModel(model);
        Assert.That(vm.Key, Is.EqualTo("secret"));
    }

    [Test]
    public void Constructor_FromModel_NullNameBecomesEmpty()
    {
        var model = new ChannelSettings { Name = null!, Key = "k", AutoJoin = true };
        var vm = new ChannelSettingsViewModel(model);
        Assert.That(vm.Name, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Constructor_FromModel_NullKeyBecomesEmpty()
    {
        var model = new ChannelSettings { Name = "#chan", Key = null!, AutoJoin = false };
        var vm = new ChannelSettingsViewModel(model);
        Assert.That(vm.Key, Is.EqualTo(string.Empty));
    }

    // ── IsKeyed: what puts the padlock on the chip ───────────────────────────

    [Test]
    public void IsKeyed_WithAKey_IsTrue()
    {
        Assert.That(new ChannelSettingsViewModel { Key = "hunter2" }.IsKeyed, Is.True);
    }

    [Test]
    public void IsKeyed_WithoutAKey_IsFalse()
    {
        Assert.That(new ChannelSettingsViewModel { Key = string.Empty }.IsKeyed, Is.False);
    }

    [Test]
    public void IsKeyed_ChangesWithTheKey()
    {
        var vm = new ChannelSettingsViewModel();
        bool raised = false;
        vm.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(vm.IsKeyed);

        vm.Key = "hunter2";

        Assert.That(raised, Is.True, "the padlock has to appear when a key is typed");
    }

    // ── ToModel ──────────────────────────────────────────────────────────────

    [Test]
    public void ToModel_RoundTrip_PreservesName()
    {
        var vm = new ChannelSettingsViewModel { Name = "#test", Key = "pw" };
        Assert.That(vm.ToModel().Name, Is.EqualTo("#test"));
    }

    [Test]
    public void ToModel_RoundTrip_PreservesKey()
    {
        var vm = new ChannelSettingsViewModel { Name = "#test", Key = "pw" };
        Assert.That(vm.ToModel().Key, Is.EqualTo("pw"));
    }

    [Test]
    public void ToModel_EmptyKey_PreservesEmpty()
    {
        var vm = new ChannelSettingsViewModel { Name = "#test", Key = string.Empty };
        Assert.That(vm.ToModel().Key, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ToModel_AlwaysAutoJoins()
    {
        // Membership of the network's channel list is the autojoin flag, so a chip that
        // exists is a channel we join. Removing the chip is how you stop.
        var vm = new ChannelSettingsViewModel { Name = "#test" };
        Assert.That(vm.ToModel().AutoJoin, Is.True);
    }
}

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
    public void Constructor_FromModel_CopiesAutoJoin()
    {
        var model = new ChannelSettings("#mutiny", true, "desc", "secret");
        var vm = new ChannelSettingsViewModel(model);
        Assert.That(vm.AutoJoin, Is.True);
    }

    [Test]
    public void Constructor_FromModel_AutoJoinFalse()
    {
        var model = new ChannelSettings("#mutiny", false, "desc", "");
        var vm = new ChannelSettingsViewModel(model);
        Assert.That(vm.AutoJoin, Is.False);
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

    [Test]
    public void ToModel_RoundTrip_PreservesName()
    {
        var vm = new ChannelSettingsViewModel { Name = "#test", Key = "pw", AutoJoin = true };
        var model = vm.ToModel();
        Assert.That(model.Name, Is.EqualTo("#test"));
    }

    [Test]
    public void ToModel_RoundTrip_PreservesKey()
    {
        var vm = new ChannelSettingsViewModel { Name = "#test", Key = "pw", AutoJoin = true };
        var model = vm.ToModel();
        Assert.That(model.Key, Is.EqualTo("pw"));
    }

    [Test]
    public void ToModel_RoundTrip_PreservesAutoJoin()
    {
        var vm = new ChannelSettingsViewModel { Name = "#test", Key = "pw", AutoJoin = true };
        var model = vm.ToModel();
        Assert.That(model.AutoJoin, Is.True);
    }

    [Test]
    public void ToModel_AutoJoinFalse_PreservedThroughRoundTrip()
    {
        var vm = new ChannelSettingsViewModel { Name = "#test", Key = "pw", AutoJoin = false };
        var model = vm.ToModel();
        Assert.That(model.AutoJoin, Is.False);
    }

    [Test]
    public void ToModel_EmptyKey_PreservesEmpty()
    {
        var vm = new ChannelSettingsViewModel { Name = "#test", Key = string.Empty, AutoJoin = true };
        var model = vm.ToModel();
        Assert.That(model.Key, Is.EqualTo(string.Empty));
    }
}
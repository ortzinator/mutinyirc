using System;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

[TestFixture]
public class ChatItemViewModelTests
{
    private static readonly DateTime SampleTime = new DateTime(2026, 4, 21, 12, 0, 0, DateTimeKind.Utc);

    // ── ChatItemViewModel ────────────────────────────────────────────────────

    [Test]
    public void ChatItemViewModel_StoresTime()
    {
        var vm = new IrcErrorViewModel(SampleTime, "err msg", "404");
        Assert.That(vm.Time, Is.EqualTo(SampleTime));
    }

    [Test]
    public void ChatItemViewModel_StoresMessage()
    {
        var vm = new IrcErrorViewModel(SampleTime, "something went wrong", "500");
        Assert.That(vm.Message, Is.EqualTo("something went wrong"));
    }

    // ── ChannelMessageViewModel ──────────────────────────────────────────────

    [Test]
    public void ChannelMessageViewModel_StoresTime()
    {
        var vm = new ChannelMessageViewModel(SampleTime, "hello", "alice");
        Assert.That(vm.Time, Is.EqualTo(SampleTime));
    }

    [Test]
    public void ChannelMessageViewModel_StoresMessage()
    {
        var vm = new ChannelMessageViewModel(SampleTime, "hello world", "alice");
        Assert.That(vm.Message, Is.EqualTo("hello world"));
    }

    [Test]
    public void ChannelMessageViewModel_SetsUserNickFromStringCtor()
    {
        var vm = new ChannelMessageViewModel(SampleTime, "hi", "bob");
        Assert.That(vm.User, Is.Not.Null);
        Assert.That(vm.User.Nick, Is.EqualTo("bob"));
    }

    [Test]
    public void ChannelMessageViewModel_SetsUserFromUserCtor()
    {
        var user = new User { Nick = "carol" };
        var vm = new ChannelMessageViewModel(SampleTime, "hi", user);
        Assert.That(vm.User, Is.SameAs(user));
    }

    // ── ChannelActionViewModel ───────────────────────────────────────────────

    [Test]
    public void ChannelActionViewModel_StoresTime()
    {
        var vm = new ChannelActionViewModel(SampleTime, "waves", "dave");
        Assert.That(vm.Time, Is.EqualTo(SampleTime));
    }

    [Test]
    public void ChannelActionViewModel_StoresMessage()
    {
        var vm = new ChannelActionViewModel(SampleTime, "waves hello", "dave");
        Assert.That(vm.Message, Is.EqualTo("waves hello"));
    }

    [Test]
    public void ChannelActionViewModel_SetsUserNickFromStringCtor()
    {
        var vm = new ChannelActionViewModel(SampleTime, "waves", "eve");
        Assert.That(vm.User, Is.Not.Null);
        Assert.That(vm.User.Nick, Is.EqualTo("eve"));
    }

    [Test]
    public void ChannelActionViewModel_IsChannelMessageViewModel()
    {
        var vm = new ChannelActionViewModel(SampleTime, "waves", "frank");
        Assert.That(vm, Is.InstanceOf<ChannelMessageViewModel>());
    }

    // ── IrcErrorViewModel ────────────────────────────────────────────────────

    [Test]
    public void IrcErrorViewModel_StoresTime()
    {
        var vm = new IrcErrorViewModel(SampleTime, "no such nick", "401");
        Assert.That(vm.Time, Is.EqualTo(SampleTime));
    }

    [Test]
    public void IrcErrorViewModel_StoresMessage()
    {
        var vm = new IrcErrorViewModel(SampleTime, "no such nick", "401");
        Assert.That(vm.Message, Is.EqualTo("no such nick"));
    }

    [Test]
    public void IrcErrorViewModel_StoresCode()
    {
        var vm = new IrcErrorViewModel(SampleTime, "no such nick", "401");
        Assert.That(vm.Code, Is.EqualTo("401"));
    }

    [Test]
    public void IrcErrorViewModel_IsChatItemViewModel()
    {
        var vm = new IrcErrorViewModel(SampleTime, "err", "500");
        Assert.That(vm, Is.InstanceOf<ChatItemViewModel>());
    }
}

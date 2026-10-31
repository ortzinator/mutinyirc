using System;
using System.IO;
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
/// Covers what a Networks dialog save does to a live connection. The connection gets the new
/// entry points of its bound network, the session stays up, and the server panel gets one line
/// when the save changed or removed the entry point that the connection is on.
/// </summary>
[TestFixture]
public class ServerViewModelEntryPointSaveTests
{
    private static readonly FieldInfo SingletonField =
        typeof(IrcSettingsManager).GetField("Singleton", BindingFlags.NonPublic | BindingFlags.Static)!;

    private IrcSettingsManager _settings = null!;
    private IConnection _connection = null!;
    private ServerViewModel _vm = null!;
    private NetworkSettings _network = null!;

    [SetUp]
    public void Setup()
    {
        // A file of this fixture's own: the test must never reach the user's servers.json.
        _settings = new IrcSettingsManager(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json"));
        SingletonField.SetValue(null, _settings);

        // The nick is set, so the dial list does not read the app settings.
        _network = _settings.GetOrAddNetwork("Libera");
        _network.Nick = "me";
        _network.AddServer(new ServerSettings("irc.libera.chat", 6697, true));
        _network.AddServer(new ServerSettings("irc.eu.libera.chat", 6697, true));

        _connection = A.Fake<IConnection>();
        A.CallTo(() => _connection.Listener).Returns(new Listener());
        A.CallTo(() => _connection.Connected).Returns(true);
        A.CallTo(() => _connection.ConnectionData)
            .Returns(new ConnectionArgs("me", "irc.libera.chat", true) { Port = 6697 });

        _vm = new ServerViewModel(new Server(_connection), new PluginManager());
        _vm.BindNetwork(_network);
    }

    [TearDown]
    public void Teardown()
    {
        _vm.Dispose();
        SingletonField.SetValue(null, null);
    }

    private int NoticeCount() =>
        _vm.ChatLines.Count(line => line.Message == ServerStrings.EntryPointChangedMessage);

    private ServerSettingsViewModel CurrentRow(SettingsViewModel dialog) =>
        dialog.Networks[0].Servers.First(s => s.Url == "irc.libera.chat");

    [AvaloniaTest]
    public void ASave_KeepsTheSessionUp()
    {
        var dialog = new SettingsViewModel(_settings);
        CurrentRow(dialog).Port = 7000;
        dialog.Save();

        _vm.FollowSavedEntryPoints();

        A.CallTo(() => _connection.Disconnect(A<string>._)).MustNotHaveHappened();
    }

    [AvaloniaTest]
    public void ASaveThatEditsTheCurrentEntryPoint_ShowsTheNoticeOnce()
    {
        var dialog = new SettingsViewModel(_settings);
        CurrentRow(dialog).Port = 7000;
        dialog.Save();

        _vm.FollowSavedEntryPoints();
        _vm.FollowSavedEntryPoints();

        Assert.That(NoticeCount(), Is.EqualTo(1));
    }

    [AvaloniaTest]
    public void ASaveThatRemovesTheCurrentEntryPoint_ShowsTheNotice()
    {
        _network.RemoveServer(_network.Servers[0]);

        _vm.FollowSavedEntryPoints();

        Assert.That(NoticeCount(), Is.EqualTo(1));
    }

    [AvaloniaTest]
    public void ASaveThatReordersOrEditsAnotherEntryPoint_ShowsNoNotice()
    {
        ServerSettings other = _network.Servers[1];
        _network.RemoveServer(other);
        _network.Servers.Insert(0, new ServerSettings(other.Url, 7000, false));

        _vm.FollowSavedEntryPoints();

        Assert.That(NoticeCount(), Is.Zero);
    }

    [AvaloniaTest]
    public void DialogSave_TellsItsListenersAfterItAppliesTheEdits()
    {
        var dialog = new SettingsViewModel(_settings);
        CurrentRow(dialog).Port = 7000;
        int portWhenTold = 0;
        int told = 0;
        dialog.NetworksApplied += (_, _) =>
        {
            told++;
            portWhenTold = _network.Servers[0].Port;
        };

        dialog.Save();

        Assert.Multiple(() =>
        {
            Assert.That(told, Is.EqualTo(1));
            Assert.That(portWhenTold, Is.EqualTo(7000));
        });
    }
}

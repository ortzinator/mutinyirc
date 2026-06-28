using Ninject;
using Ninject.Parameters;

namespace MutinyIRC.UI.ViewModels;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FlamingIRC;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Common;
using PluginFramework;

public class MainViewModel : ViewModelBase
{
    private PluginManager _pluginManager;
    private Dictionary<Server, ServerViewModel> _serverMap = new();

    public MTObservableCollection<IrcViewModel> Panels { get; protected set; }
    public MTObservableCollection<ServerViewModel> Servers { get; }

    private IrcViewModel? _selectedPanel;
    public IrcViewModel? SelectedPanel
    {
        get => _selectedPanel;
        set => SetProperty(ref _selectedPanel, value);
    }

    private RelayCommand<IrcViewModel?>? _selectPanelCommand;
    public System.Windows.Input.ICommand SelectPanelCommand =>
        _selectPanelCommand ??= new RelayCommand<IrcViewModel?>(SelectPanel);

    /// <summary>
    ///   Makes <paramref name="panel"/> the active panel, deselecting whichever panel was
    ///   previously selected. Passing null clears the selection.
    /// </summary>
    private void SelectPanel(IrcViewModel? panel)
    {
        if (_selectedPanel != null) _selectedPanel.IsSelected = false;
        SelectedPanel = panel;
        if (panel != null) panel.IsSelected = true;
    }

    public MainViewModel(PluginManager pluginManager)
    {
        _pluginManager = pluginManager;
        Panels = new MTObservableCollection<IrcViewModel>();
        Servers = new MTObservableCollection<ServerViewModel>();

        ServerManager.Instance.ServerAdded += Instance_ServerCreated;
        Server.ChannelRemoved += Server_ChannelRemoved;

        LoadSettings();

        List<ServerSettings> servers = IrcSettingsManager.Instance.GetAutoConnectServers();

        foreach (ServerSettings server in servers)
        {
            if (server.Nick == null)
                server.Nick = AppSettings.Instance.FirstNick;

            Server newServer = ServerManager.Instance.Create(new ConnectionArgs(server.Nick, server.Url, server.Ssl));
            newServer.JoinSelf += Server_JoinSelf;
            newServer.Connect();
        }

        _pluginManager.LoadPlugins(Path.Combine(AppContext.BaseDirectory, "plugins"));
        _ = RandomMessages.Instance;
    }

    private void Server_JoinSelf(object? sender, Common.DataEventArgs<Channel> e)
    {
        // A reconnect (or any rejoin) re-fires JoinSelf for a channel whose panel is still
        // open: the Channel object survives in Server.Channels across a dropped connection, so
        // CreateChannel hands back the existing instance. Reuse its panel instead of opening a
        // duplicate; just bring it back to the foreground.
        var existing = Panels.OfType<ChannelViewModel>()
            .FirstOrDefault(c => c.Channel == e.Data);
        if (existing != null)
        {
            SelectPanel(existing);
            return;
        }

        var chan = CompositionRoot.Resolve<ChannelViewModel>(new ConstructorArgument("channel", e.Data));
        chan.RequestClose += Chan_RequestClose;
        Panels.Add(chan);

        if (_serverMap.TryGetValue(e.Data.Server, out var serverVm))
            serverVm.Channels.Add(chan);

        SelectPanel(chan);
    }

    private void Server_PrivateNotice(object? sender, UserMessageEventArgs e)
    {
        var server = (Server)sender!;
        _serverMap.TryGetValue(server, out ServerViewModel? serverVm);

        // Pre-registration / server notices (no sender nick) are connection-level, so they
        // belong in the server window rather than wherever the user happens to be looking.
        if (string.IsNullOrEmpty(e.User.Nick))
        {
            serverVm?.AddServerNotice(e.Message);
            return;
        }

        // A user notice surfaces in the active window when that panel belongs to the same
        // connection, so it appears where your attention is; otherwise it falls back to the
        // notice's own server window.
        IrcViewModel? target = SelectedPanel?.OwningServer == server ? SelectedPanel : serverVm;
        target?.AddPrivateNotice(e.User.Nick, e.Message);
    }

    private void Server_AwayReplyReceived(object? sender, AwayEventArgs e)
    {
        var server = (Server)sender!;
        if (!_serverMap.TryGetValue(server, out ServerViewModel? serverVm))
            return;

        // Suppress a repeat of the same away message for this nick so messaging an away user
        // again doesn't re-print it.
        if (!serverVm.ShouldShowAwayReply(e.Nick, e.AwayMessage))
            return;

        // Surface in the active window when it belongs to this connection, so it appears where
        // your attention is; otherwise fall back to the connection's server window.
        IrcViewModel? target = SelectedPanel?.OwningServer == server ? SelectedPanel : serverVm;
        target?.AddAwayReply(e.Nick, e.AwayMessage);
    }

    private void Server_PrivateMessageSessionAdded(object? sender, PrivateMessageSessionEventArgs e)
    {
        var pm = CompositionRoot.Resolve<PrivateMessageViewModel>(
            new ConstructorArgument("session", e.PrivateMessageSession));
        pm.RequestClose += Pm_RequestClose;
        Panels.Add(pm);

        if (_serverMap.TryGetValue(e.PrivateMessageSession.Server, out var serverVm))
            serverVm.PrivateMessages.Add(pm);

        // Intentionally do NOT focus: incoming PMs must not steal focus.
    }

    private void Pm_RequestClose(object? sender, EventArgs e)
    {
        var pm = (PrivateMessageViewModel)sender!;
        pm.RequestClose -= Pm_RequestClose;
        Panels.Remove(pm);

        foreach (var sv in Servers)
            sv.PrivateMessages.Remove(pm);

        SelectNextAfterClosing(pm);

        pm.Session.Server.RemovePM(pm.Session);
        pm.Dispose();
    }

    private void Chan_RequestClose(object? sender, EventArgs e)
    {
        var chan = (ChannelViewModel)sender!;
        chan.RequestClose -= Chan_RequestClose;
        Panels.Remove(chan);

        foreach (var sv in Servers)
            sv.Channels.Remove(chan);

        SelectNextAfterClosing(chan);

        chan.Dispose();
    }

    /// <summary>
    ///   When the panel being closed is the selected one, deselects it and selects the
    ///   first remaining panel (if any). Call after the panel has been removed from
    ///   <see cref="Panels"/> so the replacement is picked from what's left.
    /// </summary>
    private void SelectNextAfterClosing(IrcViewModel closed)
    {
        if (SelectedPanel != closed)
            return;

        closed.IsSelected = false;
        var next = Panels.Count > 0 ? Panels[0] : null;
        SelectedPanel = next;
        if (next != null) next.IsSelected = true;
    }

    private void LoadSettings()
    {
        TextLoggerManager.LoggerActive = AppSettings.Instance.LoggerActivated;
        TextLoggerManager.AddTimestamp = AppSettings.Instance.LoggerTimestampsActivated;
        TextLoggerManager.TimeFormat = AppSettings.Instance.LoggerTimestampFormat;
    }

    private void Instance_ServerCreated(object? sender, ServerEventArgs e)
    {
        // Seed this connection's service-nick filter from user config.
        foreach (string nick in AppSettings.Instance.ServiceNicks)
            e.Server.ServiceNicks.Add(nick);

        CreateServerPanel(e.Server);
    }

    private void CreateServerPanel(Server server)
    {
        var vm = new ServerViewModel(server, _pluginManager);
        _serverMap[server] = vm;
        Panels.Add(vm);
        Servers.Add(vm);

        server.PrivateMessageSessionAdded += Server_PrivateMessageSessionAdded;
        server.PrivateNotice += Server_PrivateNotice;
        server.AwayReplyReceived += Server_AwayReplyReceived;

        if (SelectedPanel == null)
        {
            SelectedPanel = vm;
            vm.IsSelected = true;
        }
    }

    private void Server_ChannelRemoved(object? sender, ChannelEventArgs e)
    {
        var chanVm = Panels.OfType<ChannelViewModel>()
            .FirstOrDefault(c => c.Channel == e.Channel);
        if (chanVm != null)
            Chan_RequestClose(chanVm, EventArgs.Empty);
    }

    public override void Close()
    {
        for (int i = 0; i < Panels.Count; i++)
            Panels[i].Close();

        // App shutdown: dispose each server panel (detaching its handlers) and tear down the
        // underlying Server so it unhooks its FlamingIRC subscriptions and drops out of
        // ServerManager. Without this the ServerManager singleton's strong reference would pin
        // every Server for the process lifetime and the finalizer would never run.
        foreach (ServerViewModel serverVm in Servers.ToList())
        {
            serverVm.Dispose();
            serverVm.OwningServer?.Dispose();
        }

        base.Close();
    }
}

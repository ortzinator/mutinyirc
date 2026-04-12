using Ninject;
using Ninject.Parameters;

namespace OrtzIRC.Avalonia.ViewModels;

using System;
using System.Collections.Generic;
using System.IO;
using FlamingIRC;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrtzIRC.Common;
using OrtzIRC.PluginFramework;

public class MainViewModel : ViewModelBase
{
    private PluginManager _pluginManager;
    public MTObservableCollection<IrcViewModel> Panels { get; protected set; }

    private IrcViewModel? _selectedPanel;
    public IrcViewModel? SelectedPanel
    {
        get => _selectedPanel;
        set => SetProperty(ref _selectedPanel, value);
    }

    public MainViewModel(PluginManager pluginManager)
    {
        _pluginManager = pluginManager;
        Panels = new MTObservableCollection<IrcViewModel>();

        ServerManager.Instance.ServerAdded += Instance_ServerCreated;

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

        _pluginManager.LoadPlugins(Path.Combine(Environment.CurrentDirectory, "plugins"));
        RandomMessages.Load();
    }

    private void Server_JoinSelf(object? sender, OrtzIRC.Common.DataEventArgs<Channel> e)
    {
        var chan = CompositionRoot.Resolve<ChannelViewModel>(new ConstructorArgument("channel", e.Data));
        chan.RequestClose += Chan_RequestClose;
        Panels.Add(chan);
    }

    private void Chan_RequestClose(object? sender, EventArgs e)
    {
        var chan = (ChannelViewModel)sender!;
        chan.RequestClose -= Chan_RequestClose;
        Panels.Remove(chan);
        if (SelectedPanel == chan)
            SelectedPanel = Panels.Count > 0 ? Panels[0] : null;
    }

    private void LoadSettings()
    {
        TextLoggerManager.LoggerActive = AppSettings.Instance.LoggerActivated;
        TextLoggerManager.AddTimestamp = AppSettings.Instance.LoggerTimestampsActivated;
        TextLoggerManager.TimeFormat = AppSettings.Instance.LoggerTimestampFormat;
    }

    private void Instance_ServerCreated(object? sender, ServerEventArgs e)
    {
        CreateServerPanel(e.Server);
    }

    private void CreateServerPanel(Server server)
    {
        var vm = new ServerViewModel(server);
        Panels.Add(vm);
        if (SelectedPanel == null)
            SelectedPanel = vm;
    }

    public override void Close()
    {
        for (int i = 0; i < Panels.Count; i++)
            Panels[i].Close();

        base.Close();
    }
}

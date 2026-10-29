using System.IO;

namespace MutinyIRC.UI.ViewModels.Design;

/// <summary>
/// Fills the Networks window in the previewer. It edits a manager of its own, pointed at a file
/// in the temp directory, so the previewer neither reads nor writes the user's servers.json.
/// </summary>
public class DesignSettingsViewModel : SettingsViewModel
{
    public DesignSettingsViewModel()
        : base(new IrcSettingsManager(Path.Combine(Path.GetTempPath(), "MutinyIRC.design.json")))
    {
        var libera = new NetworkSettingsViewModel
        {
            Name = "Libera.Chat",
            Nick = "sparkplug",
            AlternateNick = "sparkplug_",
            AutoConnect = true,
        };
        libera.Servers.Add(new ServerSettingsViewModel
        { Url = "irc.libera.chat", Port = 6697, Ssl = true, Order = 1, IsFirst = true });
        libera.Servers.Add(new ServerSettingsViewModel
        {
            Url = "irc.eu.libera.chat",
            Port = 6697,
            Ssl = true,
            Order = 2,
            PinnedFingerprint = "3A7F0C9E51B2D84466E1A0F3C7D92B580E4A1C6F93D0B7E25A8C4F1D06B9E372",
        });
        libera.Servers.Add(new ServerSettingsViewModel
        { Url = "irc.libera.chat", Port = 6667, Ssl = false, Order = 3 });
        libera.Channels.Add(new ChannelSettingsViewModel { Name = "#mutinyirc" });
        libera.Channels.Add(new ChannelSettingsViewModel { Name = "#avalonia" });
        libera.Channels.Add(new ChannelSettingsViewModel { Name = "#backstage", Key = "hunter2" });

        var oftc = new NetworkSettingsViewModel { Name = "OFTC" };
        oftc.Servers.Add(new ServerSettingsViewModel
        { Url = "irc.oftc.net", Port = 6697, Ssl = true, Order = 1, IsFirst = true });

        Networks.Add(libera);
        Networks.Add(oftc);
        SelectedNetwork = libera;
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MutinyIRC.Common;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.Models;

/// <summary>
/// Pins the shape of servers.json. <c>IrcSettingsManager</c> writes a
/// <see cref="NetworkSettingsList"/> and reads back a <see cref="List{T}"/> of networks, so the
/// two must agree even though they are different types.
/// </summary>
[TestFixture]
public class NetworkSettingsPersistenceTests
{
    // Must match IrcSettingsManager.JsonOptions.
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };

    private static NetworkSettingsList BuildSavedList()
    {
        var libera = new NetworkSettings("Libera");
        libera.AddServer(new ServerSettings("irc.libera.chat", "Libera", "6667", false));
        libera.AddChannel(new ChannelSettings("#MutinyIRC", true));

        var list = new NetworkSettingsList();
        list.ReplaceAll(new[] { libera });
        return list;
    }

    private static List<NetworkSettings> RoundTrip(NetworkSettingsList list) =>
        JsonSerializer.Deserialize<List<NetworkSettings>>(
            JsonSerializer.Serialize(list, Options), Options)!;

    [Test]
    public void SavedList_SerializesAsAJsonArray()
    {
        string json = JsonSerializer.Serialize(BuildSavedList(), Options);

        Assert.That(json.TrimStart()[0], Is.EqualTo('['),
            "The loader reads a bare array, so the list must not serialize as an object wrapper");
    }

    [Test]
    public void RoundTrip_KeepsNetworksEntryPointsAndChannels()
    {
        List<NetworkSettings> loaded = RoundTrip(BuildSavedList());

        Assert.That(loaded.Select(n => n.Name), Is.EqualTo(new[] { "Libera" }));
        Assert.That(loaded[0].Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc.libera.chat" }));
        Assert.That(loaded[0].Channels.Select(c => c.Name), Is.EqualTo(new[] { "#MutinyIRC" }));
        Assert.That(loaded[0].Channels[0].AutoJoin, Is.True);
    }

    [Test]
    public void NullCollectionsInJson_ReadAsEmptyRatherThanNull()
    {
        // A hand-edited or truncated file must not hand every reader a null list to guard.
        const string json = """[{"Name":"Libera","Servers":null,"Channels":null}]""";

        List<NetworkSettings> loaded =
            JsonSerializer.Deserialize<List<NetworkSettings>>(json, Options)!;

        Assert.That(loaded[0].Servers, Is.Empty);
        Assert.That(loaded[0].Channels, Is.Empty);
    }
}

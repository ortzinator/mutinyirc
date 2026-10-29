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
        libera.AddServer(new ServerSettings("irc.libera.chat", 6667, false));
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
    public void RoundTrip_KeepsTheVerifySwitchOff()
    {
        NetworkSettingsList list = BuildSavedList();
        list[0].VerifyCertificates = false;

        Assert.That(RoundTrip(list)[0].VerifyCertificates, Is.False);
    }

    [Test]
    public void ANetworkWithNoVerifyField_Verifies()
    {
        const string json = """[{ "Name": "Libera", "Servers": [], "Channels": [] }]""";

        var loaded = JsonSerializer.Deserialize<List<NetworkSettings>>(json, Options)!;

        Assert.That(loaded[0].VerifyCertificates, Is.True,
            "a servers.json written before the switch existed must load with verification on");
    }

    // ── The shape written before a network owned the port, nick and autoconnect ──

    /// <summary>
    /// A servers.json as it was written when every entry point carried its own port list, its
    /// own nick and its own autoconnect flag.
    /// </summary>
    private const string LegacyJson = """
        [{
          "Name": "Libera",
          "Servers": [
            { "Url": "irc.libera.chat", "Description": "Libera", "Ports": "6697,6667",
              "Ssl": true, "AutoConnect": false, "Nick": "sparkplug" },
            { "Url": "eu.libera.chat", "Description": "", "Ports": "6667",
              "Ssl": false, "AutoConnect": true, "Nick": null }
          ],
          "Channels": [ { "Name": "#MutinyIRC", "AutoJoin": true, "Key": "", "Description": "" } ]
        }]
        """;

    private static NetworkSettings LoadLegacy()
    {
        List<NetworkSettings> loaded =
            JsonSerializer.Deserialize<List<NetworkSettings>>(LegacyJson, Options)!;
        IrcSettingsManager.FoldLegacyEntryPointFields(loaded);
        return loaded[0];
    }

    [Test]
    public void LegacyPortsString_BecomesTheFirstPort()
    {
        NetworkSettings network = LoadLegacy();

        Assert.That(network.Servers.Select(s => s.Port), Is.EqualTo(new[] { 6697, 6667 }));
    }

    [Test]
    public void LegacyAutoConnect_FoldsUpOntoTheNetwork()
    {
        // A network dials if any host it listed used to, because one network is now one
        // connection rather than one connection per flagged host.
        Assert.That(LoadLegacy().AutoConnect, Is.True);
    }

    [Test]
    public void LegacyNick_FoldsUpOntoTheNetwork()
    {
        Assert.That(LoadLegacy().Nick, Is.EqualTo("sparkplug"));
    }

    [Test]
    public void LegacyFields_AreNotWrittenBackOut()
    {
        // The fold is one-way: the next save must drop the fields it read, or the file
        // keeps two answers to the same question.
        NetworkSettings network = LoadLegacy();
        var list = new NetworkSettingsList();
        list.ReplaceAll(new[] { network });

        string json = JsonSerializer.Serialize(list, Options);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Not.Contain("\"Ports\""),
                "the old port list has to leave the file, or it holds a second answer");
            Assert.That(json, Does.Contain("\"Port\": 6697"));
            Assert.That(json, Does.Contain("\"AutoConnect\": true"),
                "the network keeps the flag it was handed");
            Assert.That(json, Does.Not.Contain("\"AutoConnect\": false"),
                "the entry point no longer carries one");
        });
    }

    [Test]
    public void LegacyFile_RoundTripsIntoTheNewShapeUnchanged()
    {
        NetworkSettings folded = LoadLegacy();
        var list = new NetworkSettingsList();
        list.ReplaceAll(new[] { folded });

        List<NetworkSettings> reloaded = RoundTrip(list);
        IrcSettingsManager.FoldLegacyEntryPointFields(reloaded);

        Assert.Multiple(() =>
        {
            Assert.That(reloaded[0].Servers.Select(s => s.Port), Is.EqualTo(new[] { 6697, 6667 }));
            Assert.That(reloaded[0].AutoConnect, Is.True);
            Assert.That(reloaded[0].Nick, Is.EqualTo("sparkplug"));
        });
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

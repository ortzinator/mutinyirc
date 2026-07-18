using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MutinyIRC.UI;

public sealed class AppSettings
{
    private static AppSettings? _instance;
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "MutinyIRC", "settings.json");

    public static AppSettings Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = Load();
            return _instance;
        }
    }

    public bool ShowTimestamps { get; set; } = true;
    public bool LoggerActivated { get; set; } = false;
    public bool LoggerTimestampsActivated { get; set; } = true;
    public string LoggerTimestampFormat { get; set; } = "[hh:mm:ss]";
    public string FirstNick { get; set; } = "MutinyIRC";
    public string SecondNick { get; set; } = "MutinyIRC_";
    public string ThirdNick { get; set; } = "MutinyIRC__";

    /// <summary>
    /// Nicknames treated as server-side services. PRIVMSGs to or from these nicks
    /// bypass the PM tab UI and are routed to the server window. Edit the JSON file
    /// directly to customise; there is intentionally no settings UI.
    /// </summary>
    public List<string> ServiceNicks { get; set; } = new()
    {
        "ChanServ", "NickServ", "MemoServ", "OperServ",
        "BotServ", "HostServ", "HelpServ", "SaslServ",
        "AuthServ", "Global", "Q"
    };

    private static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not load settings: {ex.Message}");
        }

        // First run (or unreadable file) — write defaults out so the user can
        // discover and hand-edit fields that have no in-app UI (e.g. ServiceNicks).
        var defaults = new AppSettings();
        defaults.Save();
        return defaults;
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not save settings: {ex.Message}");
        }
    }
}

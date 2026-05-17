namespace OrtzIRC.Avalonia;

using System;
using System.IO;
using System.Text.Json;

public sealed class AppSettings
{
    private static AppSettings? _instance;
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "OrtzIRC", "settings.json");

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
    public string FirstNick { get; set; } = "OrtzIRC";
    public string SecondNick { get; set; } = "OrtzIRC_";
    public string ThirdNick { get; set; } = "OrtzIRC__";

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
        return new AppSettings();
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
